from __future__ import annotations

import argparse
import json
import sys
from datetime import datetime
from typing import Any

from lcu_client import LcuClient


def dotnet_date(iso_value: str) -> str:
    return datetime.fromisoformat(iso_value).astimezone().isoformat(
        timespec="seconds"
    )


def named_objects(values: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return [
        {"Id": int(item.get("id") or 0), "Name": str(item.get("name") or "")}
        for item in values
    ]


def convert_player(item: dict[str, Any]) -> dict[str, Any]:
    return {
        "RiotId": str(item.get("riot_id") or "未知玩家"),
        "SubteamId": int(item.get("subteam_id") or 0),
        "ChampionId": int(item.get("champion_id") or 0),
        "ChampionName": str(item.get("champion_name") or ""),
        "Placement": item.get("placement"),
        "Kills": item.get("kills"),
        "Deaths": item.get("deaths"),
        "Assists": item.get("assists"),
        "ChampionLevel": item.get("champion_level"),
        "GoldEarned": item.get("gold_earned"),
        "DamageToChampions": item.get("damage_to_champions"),
        "DamageTaken": item.get("damage_taken"),
        "TotalHeal": item.get("total_heal"),
        "DamageSelfMitigated": item.get("damage_self_mitigated"),
        "Items": named_objects(item.get("items") or []),
        "Augments": named_objects(item.get("augments") or []),
    }


def convert_match(item: dict[str, Any]) -> dict[str, Any]:
    return {
        "GameId": str(item["game_id"]),
        "Source": str(item.get("source") or "auto"),
        "PlayedAt": dotnet_date(item["played_at"]),
        "DurationSeconds": item.get("duration_seconds"),
        "GameVersion": str(item.get("game_version") or ""),
        "QueueId": int(item.get("queue_id") or 0),
        "GameMode": str(item.get("game_mode") or ""),
        "ChampionId": int(item.get("champion_id") or 0),
        "ChampionName": str(item.get("champion_name") or ""),
        "Placement": int(item["placement"]),
        "Kills": item.get("kills"),
        "Deaths": item.get("deaths"),
        "Assists": item.get("assists"),
        "ChampionLevel": item.get("champion_level"),
        "GoldEarned": item.get("gold_earned"),
        "DamageToChampions": item.get("damage_to_champions"),
        "DamageTaken": item.get("damage_taken"),
        "TotalHeal": item.get("total_heal"),
        "DamageSelfMitigated": item.get("damage_self_mitigated"),
        "Items": named_objects(item.get("items") or []),
        "Augments": named_objects(item.get("augments") or []),
        "ParticipantDetailsLoaded": bool(
            item.get("participant_details_loaded")
        ),
        "Teammates": [
            convert_player(player) for player in item.get("teammates") or []
        ],
        "Opponents": [
            convert_player(player) for player in item.get("opponents") or []
        ],
    }


def write_response(payload: dict[str, Any]) -> None:
    encoded = json.dumps(
        payload, ensure_ascii=False, separators=(",", ":")
    ).encode("utf-8")
    sys.stdout.buffer.write(encoded)
    sys.stdout.buffer.flush()


def main() -> int:
    parser = argparse.ArgumentParser(add_help=False)
    parser.add_argument("--client-root", default="")
    parser.add_argument("--limit", type=int, default=100)
    parser.add_argument("--known-details", default="")
    parser.add_argument("--detail-limit", type=int, default=12)
    parser.add_argument("--icon-dir", default="")
    parser.add_argument("--icon-ids", default="")
    parser.add_argument("--account-only", action="store_true")
    parser.add_argument("--claim-game-ids", default="")
    args = parser.parse_args()
    known_details = {
        value.strip()
        for value in args.known_details.split(",")
        if value.strip()
    }
    requested_icon_ids = {
        int(value)
        for value in args.icon_ids.split(",")
        if value.strip().isdigit() and int(value) > 0
    }
    try:
        client = LcuClient(args.client_root or None)
        if args.account_only:
            current = client.get("/lol-summoner/v1/current-summoner")
            write_response(
                {
                    "Success": True,
                    "Error": "",
                    "Matches": [],
                    "HistoryCount": 0,
                    "ClientRoot": str(client.client_root),
                    "IconsCached": 0,
                    "AccountPuuid": str(current.get("puuid") or ""),
                    "AccountDisplayName":
                        client._account_display_name(current),
                    "AccountProfileIconId": int(
                        current.get("profileIconId") or 0
                    ),
                    "ClaimedGameIds": [],
                    "CheckedClaimGameIds": [],
                }
            )
            return 0
        payload = client.fetch_import_payload(
            history_limit=max(1, min(args.limit, 500)),
            known_detail_game_ids=known_details,
            detail_limit=max(0, min(args.detail_limit, 50)),
            claim_game_ids=[
                value.strip()
                for value in args.claim_game_ids.split(",")
                if value.strip()
            ],
        )
        for match in payload.matches:
            requested_icon_ids.add(int(match.get("champion_id") or 0))
            for player in (
                (match.get("teammates") or [])
                + (match.get("opponents") or [])
            ):
                requested_icon_ids.add(int(player.get("champion_id") or 0))
        icons_cached = 0
        if args.icon_dir:
            icons_cached = client.cache_champion_icons(
                requested_icon_ids, args.icon_dir
            )
        write_response(
            {
                "Success": True,
                "Error": "",
                "Matches": [convert_match(item) for item in payload.matches],
                "HistoryCount": payload.history_count,
                "ClientRoot": str(payload.client_root),
                "IconsCached": icons_cached,
                "AccountPuuid": payload.account_puuid,
                "AccountDisplayName": payload.account_display_name,
                "AccountProfileIconId": payload.account_profile_icon_id,
                "ClaimedGameIds": payload.claimed_game_ids,
                "CheckedClaimGameIds":
                    payload.checked_claim_game_ids,
            }
        )
        return 0
    except Exception as exc:
        write_response(
            {
                "Success": False,
                "Error": str(exc),
                "Matches": [],
                "HistoryCount": 0,
                "ClientRoot": args.client_root,
                "AccountPuuid": "",
                "AccountDisplayName": "",
                "AccountProfileIconId": 0,
                "ClaimedGameIds": [],
                "CheckedClaimGameIds": [],
            }
        )
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
