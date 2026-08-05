from __future__ import annotations

import base64
import json
import os
import re
import ssl
import urllib.error
import urllib.request
from dataclasses import dataclass
from datetime import datetime
from pathlib import Path
from typing import Any


ARENA_QUEUE_IDS = {1700, 1710, 1750}
ARENA_GAME_MODE = "CHERRY"


class LcuError(RuntimeError):
    pass


@dataclass
class ImportPayload:
    matches: list[dict[str, Any]]
    champions: list[dict[str, Any]]
    client_root: Path
    history_count: int
    account_puuid: str
    account_display_name: str
    account_profile_icon_id: int
    claimed_game_ids: list[str]
    checked_claim_game_ids: list[str]


class LcuClient:
    def __init__(self, preferred_root: str | Path | None = None) -> None:
        self.client_root = self.find_client_root(preferred_root)
        self.port, self.token = self._read_connection_parameters()
        self.ssl_context = ssl._create_unverified_context()
        credentials = base64.b64encode(f"riot:{self.token}".encode()).decode()
        self.headers = {
            "Authorization": f"Basic {credentials}",
            "Accept": "application/json",
            "User-Agent": "ArenaTrackerCN/0.1",
        }

    @staticmethod
    def _candidate_roots() -> list[Path]:
        relative_candidates = (
            Path("wegame/WeGameApps/英雄联盟/LeagueClient"),
            Path("WeGameApps/英雄联盟/LeagueClient"),
            Path("Riot Games/League of Legends"),
            Path("Program Files/Tencent/英雄联盟/LeagueClient"),
            Path("Program Files (x86)/Tencent/英雄联盟/LeagueClient"),
        )
        candidates: list[Path] = []
        for letter in "CDEFGHI":
            drive = Path(f"{letter}:/")
            if drive.exists():
                candidates.extend(drive / relative for relative in relative_candidates)
        return candidates

    @classmethod
    def find_client_root(cls, preferred_root: str | Path | None = None) -> Path:
        candidates: list[Path] = []
        if preferred_root:
            candidates.append(Path(preferred_root))
        candidates.extend(cls._candidate_roots())

        for root in candidates:
            if (root / "LeagueClient.exe").exists():
                return root
        raise LcuError(
            "没有找到国服客户端目录。请先启动英雄联盟客户端，"
            "或点击“客户端目录”手动选择 LeagueClient 文件夹。"
        )

    def _latest_ux_log(self) -> Path:
        logs = list(self.client_root.glob("*LeagueClientUx.log"))
        if not logs:
            raise LcuError("客户端目录中没有运行日志，请确认英雄联盟客户端已经登录。")
        return max(logs, key=lambda path: path.stat().st_mtime)

    def _read_connection_parameters(self) -> tuple[int, str]:
        log_path = self._latest_ux_log()
        try:
            content = log_path.read_text(encoding="utf-8", errors="ignore")
        except OSError as exc:
            raise LcuError(f"无法读取客户端日志：{exc}") from exc

        port_match = re.search(r"--app-port[= ]+(\d{2,5})", content, re.I)
        token_match = re.search(
            r"--remoting-auth-token[= ]+([^\s]+)", content, re.I
        )
        if not port_match or not token_match:
            raise LcuError(
                "客户端日志中没有找到本地连接参数，请重启英雄联盟客户端后重试。"
            )
        token = token_match.group(1).strip("\"'")
        return int(port_match.group(1)), token

    def get(self, path: str) -> Any:
        request = urllib.request.Request(
            f"https://127.0.0.1:{self.port}{path}", headers=self.headers
        )
        try:
            with urllib.request.urlopen(
                request, context=self.ssl_context, timeout=10
            ) as response:
                return json.loads(response.read())
        except urllib.error.HTTPError as exc:
            raise LcuError(f"客户端接口返回 HTTP {exc.code}") from exc
        except (urllib.error.URLError, TimeoutError, OSError) as exc:
            raise LcuError(
                "无法连接英雄联盟客户端，请确认客户端仍在运行并已完成登录。"
            ) from exc

    def cache_champion_icons(
        self, champion_ids: set[int], icon_directory: str | Path
    ) -> int:
        directory = Path(icon_directory)
        directory.mkdir(parents=True, exist_ok=True)
        cached = 0
        for champion_id in sorted(champion_ids):
            if champion_id <= 0:
                continue
            target = directory / f"{champion_id}.png"
            if target.exists() and target.stat().st_size > 100:
                continue
            request = urllib.request.Request(
                "https://127.0.0.1:"
                f"{self.port}/lol-game-data/assets/v1/"
                f"champion-icons/{champion_id}.png",
                headers=self.headers,
            )
            temporary = target.with_suffix(".png.tmp")
            try:
                with urllib.request.urlopen(
                    request, context=self.ssl_context, timeout=10
                ) as response:
                    content = response.read()
                if not content.startswith(b"\x89PNG\r\n\x1a\n"):
                    continue
                temporary.write_bytes(content)
                os.replace(temporary, target)
                cached += 1
            except (urllib.error.URLError, TimeoutError, OSError):
                try:
                    temporary.unlink(missing_ok=True)
                except OSError:
                    pass
        return cached

    @staticmethod
    def _is_arena(game: dict[str, Any]) -> bool:
        return (
            game.get("queueId") in ARENA_QUEUE_IDS
            or game.get("gameMode") == ARENA_GAME_MODE
        )

    @staticmethod
    def _account_display_name(current: dict[str, Any]) -> str:
        game_name = str(current.get("gameName") or "").strip()
        tag_line = str(current.get("tagLine") or "").strip()
        if game_name and tag_line:
            return f"{game_name}#{tag_line}"
        return game_name or "未知账号"

    @staticmethod
    def _same_player(current: dict[str, Any], player: dict[str, Any]) -> bool:
        for field in ("summonerId", "puuid", "accountId"):
            current_value = current.get(field)
            player_value = player.get(field)
            if current_value not in (None, "") and str(current_value) == str(
                player_value
            ):
                return True
        return False

    @classmethod
    def _own_participant(
        cls, game: dict[str, Any], current: dict[str, Any]
    ) -> dict[str, Any] | None:
        participant_id = None
        for identity in game.get("participantIdentities") or []:
            if cls._same_player(current, identity.get("player") or {}):
                participant_id = identity.get("participantId")
                break
        if participant_id is None:
            return None
        return next(
            (
                participant
                for participant in game.get("participants") or []
                if participant.get("participantId") == participant_id
            ),
            None,
        )

    @staticmethod
    def _played_at(timestamp_ms: int | None) -> str:
        if not timestamp_ms:
            return datetime.now().astimezone().isoformat(timespec="seconds")
        return datetime.fromtimestamp(
            timestamp_ms / 1000
        ).astimezone().isoformat(timespec="seconds")

    @staticmethod
    def _identity_map(game: dict[str, Any]) -> dict[Any, dict[str, Any]]:
        return {
            identity.get("participantId"): identity.get("player") or {}
            for identity in game.get("participantIdentities") or []
        }

    @staticmethod
    def _riot_id(player: dict[str, Any]) -> str:
        game_name = str(player.get("gameName") or "").strip()
        tag_line = str(player.get("tagLine") or "").strip()
        if game_name and tag_line:
            return f"{game_name}#{tag_line}"
        return game_name or str(player.get("summonerName") or "未知玩家")

    @staticmethod
    def _named_objects(
        stats: dict[str, Any],
        prefix: str,
        indexes: range,
        names: dict[int, str],
    ) -> list[dict[str, Any]]:
        values: list[dict[str, Any]] = []
        for index in indexes:
            raw_id = stats.get(f"{prefix}{index}")
            if raw_id in (None, 0):
                continue
            object_id = int(raw_id)
            values.append(
                {"id": object_id, "name": names.get(object_id, str(object_id))}
            )
        return values

    @classmethod
    def _participant_record(
        cls,
        participant: dict[str, Any],
        player: dict[str, Any],
        champion_map: dict[int, str],
        item_map: dict[int, str],
        augment_map: dict[int, str],
    ) -> dict[str, Any]:
        stats = participant.get("stats") or {}
        champion_id = int(participant.get("championId") or 0)
        return {
            "riot_id": cls._riot_id(player),
            "subteam_id": int(stats.get("playerSubteamId") or 0),
            "champion_id": champion_id,
            "champion_name": champion_map.get(
                champion_id, f"英雄 {champion_id}"
            ),
            "placement": stats.get("subteamPlacement"),
            "kills": stats.get("kills"),
            "deaths": stats.get("deaths"),
            "assists": stats.get("assists"),
            "champion_level": stats.get("champLevel"),
            "gold_earned": stats.get("goldEarned"),
            "damage_to_champions": stats.get("totalDamageDealtToChampions"),
            "damage_taken": stats.get("totalDamageTaken"),
            "total_heal": stats.get("totalHeal"),
            "damage_self_mitigated": stats.get("damageSelfMitigated"),
            "items": cls._named_objects(stats, "item", range(7), item_map),
            "augments": cls._named_objects(
                stats, "playerAugment", range(1, 7), augment_map
            ),
        }

    @classmethod
    def _participant_rosters(
        cls,
        game: dict[str, Any],
        current: dict[str, Any],
        champion_map: dict[int, str],
        item_map: dict[int, str],
        augment_map: dict[int, str],
    ) -> tuple[bool, list[dict[str, Any]], list[dict[str, Any]]]:
        own = cls._own_participant(game, current)
        if not own:
            return False, [], []
        own_stats = own.get("stats") or {}
        own_subteam = own_stats.get("playerSubteamId")
        if own_subteam is None:
            return False, [], []

        identities = cls._identity_map(game)
        teammates: list[dict[str, Any]] = []
        opponents: list[dict[str, Any]] = []
        for participant in game.get("participants") or []:
            if participant.get("participantId") == own.get("participantId"):
                continue
            player_record = cls._participant_record(
                participant,
                identities.get(participant.get("participantId"), {}),
                champion_map,
                item_map,
                augment_map,
            )
            participant_subteam = (
                participant.get("stats") or {}
            ).get("playerSubteamId")
            if participant_subteam == own_subteam:
                teammates.append(player_record)
            else:
                opponents.append(player_record)
        opponents.sort(
            key=lambda item: (
                item.get("placement") if isinstance(item.get("placement"), int) else 99,
                item.get("riot_id") or "",
            )
        )
        return True, teammates, opponents

    def fetch_import_payload(
        self,
        history_limit: int = 100,
        known_detail_game_ids: set[str] | None = None,
        detail_limit: int = 12,
        claim_game_ids: list[str] | None = None,
    ) -> ImportPayload:
        current = self.get("/lol-summoner/v1/current-summoner")
        history = self.get(
            "/lol-match-history/v1/products/lol/current-summoner/"
            f"matches?begIndex=0&endIndex={int(history_limit)}"
        )
        champions = self.get("/lol-game-data/assets/v1/champion-summary.json")
        items = self.get("/lol-game-data/assets/v1/items.json")
        augments = self.get("/lol-game-data/assets/v1/cherry-augments.json")

        champion_map = {
            int(item["id"]): item.get("name") or f"英雄 {item['id']}"
            for item in champions
            if item.get("id") is not None
        }
        item_map = {
            int(item["id"]): item.get("name") or str(item["id"])
            for item in items
            if item.get("id") is not None
        }
        augment_map = {
            int(item["id"]): item.get("nameTRA") or item.get("augmentNameId")
            for item in augments
            if item.get("id") is not None
        }

        games = ((history.get("games") or {}).get("games") or [])
        known_details = known_detail_game_ids or set()
        detail_requests = 0
        records: list[dict[str, Any]] = []
        for game in games:
            if not self._is_arena(game):
                continue
            participant = self._own_participant(game, current)
            if not participant:
                continue
            stats = participant.get("stats") or {}
            placement = stats.get("subteamPlacement")
            if not isinstance(placement, int) or not 1 <= placement <= 8:
                continue

            champion_id = participant.get("championId")
            game_id = str(game.get("gameId"))
            participant_details_loaded = False
            teammates: list[dict[str, Any]] = []
            opponents: list[dict[str, Any]] = []
            if (
                game_id not in known_details
                and detail_requests < max(0, int(detail_limit))
            ):
                detail_requests += 1
                try:
                    detail_game = self.get(
                        f"/lol-match-history/v1/games/{game_id}"
                    )
                    (
                        participant_details_loaded,
                        teammates,
                        opponents,
                    ) = self._participant_rosters(
                        detail_game,
                        current,
                        champion_map,
                        item_map,
                        augment_map,
                    )
                except (LcuError, KeyError, TypeError, ValueError):
                    pass
            records.append(
                {
                    "game_id": game_id,
                    "source": "auto",
                    "played_at": self._played_at(game.get("gameCreation")),
                    "duration_seconds": game.get("gameDuration"),
                    "game_version": game.get("gameVersion") or "",
                    "queue_id": game.get("queueId"),
                    "game_mode": game.get("gameMode") or "",
                    "champion_id": champion_id,
                    "champion_name": champion_map.get(
                        champion_id, f"英雄 {champion_id}"
                    ),
                    "placement": placement,
                    "kills": stats.get("kills"),
                    "deaths": stats.get("deaths"),
                    "assists": stats.get("assists"),
                    "champion_level": stats.get("champLevel"),
                    "gold_earned": stats.get("goldEarned"),
                    "damage_to_champions": stats.get(
                        "totalDamageDealtToChampions"
                    ),
                    "damage_taken": stats.get("totalDamageTaken"),
                    "total_heal": stats.get("totalHeal"),
                    "damage_self_mitigated": stats.get(
                        "damageSelfMitigated"
                    ),
                    "items": self._named_objects(
                        stats, "item", range(7), item_map
                    ),
                    "augments": self._named_objects(
                        stats, "playerAugment", range(1, 7), augment_map
                    ),
                    "participant_details_loaded": participant_details_loaded,
                    "teammates": teammates,
                    "opponents": opponents,
                }
            )

        claimed_game_ids: list[str] = []
        checked_claim_game_ids: list[str] = []
        imported_game_ids = {
            str(record.get("game_id") or "") for record in records
        }
        for game_id in (claim_game_ids or [])[:20]:
            game_id = str(game_id).strip()
            if not game_id:
                continue
            if game_id in imported_game_ids:
                checked_claim_game_ids.append(game_id)
                claimed_game_ids.append(game_id)
                continue
            try:
                claim_game = self.get(
                    f"/lol-match-history/v1/games/{game_id}"
                )
                checked_claim_game_ids.append(game_id)
                if self._own_participant(claim_game, current):
                    claimed_game_ids.append(game_id)
            except (LcuError, KeyError, TypeError, ValueError):
                pass

        current_after = self.get("/lol-summoner/v1/current-summoner")
        initial_puuid = str(current.get("puuid") or "")
        final_puuid = str(current_after.get("puuid") or "")
        if not initial_puuid or initial_puuid != final_puuid:
            raise LcuError("检测到客户端账号发生变化，本次导入已取消。")

        return ImportPayload(
            matches=records,
            champions=[
                {"id": item["id"], "name": item["name"]}
                for item in champions
                if item.get("id") is not None and item.get("name")
            ],
            client_root=self.client_root,
            history_count=len(games),
            account_puuid=initial_puuid,
            account_display_name=self._account_display_name(current_after),
            account_profile_icon_id=int(
                current_after.get("profileIconId") or 0
            ),
            claimed_game_ids=claimed_game_ids,
            checked_claim_game_ids=checked_claim_game_ids,
        )
