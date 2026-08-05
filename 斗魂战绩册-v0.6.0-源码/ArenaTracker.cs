using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("斗魂战绩册")]
[assembly: System.Reflection.AssemblyDescription("国服斗魂竞技场个人英雄统计工具")]
[assembly: System.Reflection.AssemblyCompany("Arena Tracker CN")]
[assembly: System.Reflection.AssemblyProduct("斗魂战绩册")]
[assembly: System.Reflection.AssemblyVersion("0.6.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("0.6.0.0")]

namespace ArenaTrackerCN
{
    internal static class Theme
    {
        public static readonly Color Background = Color.FromArgb(15, 23, 42);
        public static readonly Color Panel = Color.FromArgb(23, 32, 51);
        public static readonly Color PanelAlt = Color.FromArgb(17, 27, 46);
        public static readonly Color Border = Color.FromArgb(39, 52, 74);
        public static readonly Color Text = Color.FromArgb(232, 237, 245);
        public static readonly Color Muted = Color.FromArgb(154, 167, 187);
        public static readonly Color Accent = Color.FromArgb(214, 168, 75);
        public static readonly Color AccentHover = Color.FromArgb(229, 187, 97);
        public static readonly Color Success = Color.FromArgb(56, 185, 135);
        public static readonly Color Danger = Color.FromArgb(239, 106, 114);
        public static readonly Color Selection = Color.FromArgb(40, 57, 85);

        public static Font Font(float size, FontStyle style)
        {
            return new Font("Microsoft YaHei UI", size, style, GraphicsUnit.Point);
        }

        public static Button Button(string text, Color back, Color fore)
        {
            Button button = new Button();
            button.Text = text;
            button.BackColor = back;
            button.ForeColor = fore;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor =
                back == Accent ? AccentHover : Selection;
            button.Font = Font(9.5f, FontStyle.Bold);
            button.Height = 38;
            button.AutoSize = true;
            button.Padding = new Padding(12, 0, 12, 0);
            button.Cursor = Cursors.Hand;
            return button;
        }
    }

    internal sealed class DarkMenuColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground
        {
            get { return Theme.PanelAlt; }
        }
        public override Color MenuBorder
        {
            get { return Theme.Border; }
        }
        public override Color MenuItemBorder
        {
            get { return Theme.Selection; }
        }
        public override Color MenuItemSelected
        {
            get { return Theme.Selection; }
        }
        public override Color MenuItemSelectedGradientBegin
        {
            get { return Theme.Selection; }
        }
        public override Color MenuItemSelectedGradientEnd
        {
            get { return Theme.Selection; }
        }
        public override Color ImageMarginGradientBegin
        {
            get { return Theme.PanelAlt; }
        }
        public override Color ImageMarginGradientMiddle
        {
            get { return Theme.PanelAlt; }
        }
        public override Color ImageMarginGradientEnd
        {
            get { return Theme.PanelAlt; }
        }
    }

    public sealed class MatchRecord
    {
        public string AccountKey { get; set; }
        public List<string> ClaimCheckedAccountKeys { get; set; }
        public string GameId { get; set; }
        public string Source { get; set; }
        public DateTime PlayedAt { get; set; }
        public int? DurationSeconds { get; set; }
        public string GameVersion { get; set; }
        public int QueueId { get; set; }
        public string GameMode { get; set; }
        public int ChampionId { get; set; }
        public string ChampionName { get; set; }
        public int Placement { get; set; }
        public int? Kills { get; set; }
        public int? Deaths { get; set; }
        public int? Assists { get; set; }
        public int? ChampionLevel { get; set; }
        public int? GoldEarned { get; set; }
        public int? DamageToChampions { get; set; }
        public int? DamageTaken { get; set; }
        public int? TotalHeal { get; set; }
        public int? DamageSelfMitigated { get; set; }
        public List<NamedGameObject> Items { get; set; }
        public List<NamedGameObject> Augments { get; set; }
        public bool ParticipantDetailsLoaded { get; set; }
        public List<PlayerMatchRecord> Teammates { get; set; }
        public List<PlayerMatchRecord> Opponents { get; set; }

        public MatchRecord()
        {
            AccountKey = "";
            ClaimCheckedAccountKeys = new List<string>();
            GameId = "";
            Source = "auto";
            GameVersion = "";
            GameMode = "";
            ChampionName = "";
            Items = new List<NamedGameObject>();
            Augments = new List<NamedGameObject>();
            Teammates = new List<PlayerMatchRecord>();
            Opponents = new List<PlayerMatchRecord>();
        }
    }

    public sealed class PlayerMatchRecord
    {
        public string RiotId { get; set; }
        public int SubteamId { get; set; }
        public int ChampionId { get; set; }
        public string ChampionName { get; set; }
        public int Placement { get; set; }
        public int? Kills { get; set; }
        public int? Deaths { get; set; }
        public int? Assists { get; set; }
        public int? ChampionLevel { get; set; }
        public int? GoldEarned { get; set; }
        public int? DamageToChampions { get; set; }
        public int? DamageTaken { get; set; }
        public int? TotalHeal { get; set; }
        public int? DamageSelfMitigated { get; set; }
        public List<NamedGameObject> Items { get; set; }
        public List<NamedGameObject> Augments { get; set; }

        public PlayerMatchRecord()
        {
            RiotId = "";
            ChampionName = "";
            Items = new List<NamedGameObject>();
            Augments = new List<NamedGameObject>();
        }
    }

    public sealed class NamedGameObject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public NamedGameObject()
        {
            Name = "";
        }
    }

    public sealed class AppSettings
    {
        public string ClientRoot { get; set; }
        public bool AutoSync { get; set; }
        public bool PrivacyMode { get; set; }
        public string SelectedAccountKey { get; set; }
        public List<AccountProfile> Accounts { get; set; }

        public AppSettings()
        {
            ClientRoot = "";
            AutoSync = true;
            PrivacyMode = false;
            SelectedAccountKey = "";
            Accounts = new List<AccountProfile>();
        }
    }

    public sealed class AccountProfile
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public int ProfileIconId { get; set; }
        public DateTime LastSeenAt { get; set; }
        public DateTime? LastImportAt { get; set; }

        public AccountProfile()
        {
            Key = "";
            DisplayName = "";
        }
    }

    public sealed class HeroSummary
    {
        public int ChampionId { get; set; }
        public string ChampionName { get; set; }
        public int Picks { get; set; }
        public int Wins { get; set; }
        public double WinRate { get; set; }
        public double AveragePlacement { get; set; }

        public HeroSummary()
        {
            ChampionName = "";
        }
    }

    internal static class AppPaths
    {
        public static readonly string AppDirectory =
            String.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable("ARENA_TRACKER_DATA_DIR"))
            ? Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ArenaTrackerCN")
            : Environment.GetEnvironmentVariable("ARENA_TRACKER_DATA_DIR");
        public static readonly string MatchesFile = Path.Combine(
            AppDirectory, "matches.json"
        );
        public static readonly string SettingsFile = Path.Combine(
            AppDirectory, "settings.json"
        );
        public static readonly string ChampionIconDirectory = Path.Combine(
            AppDirectory, "champion-icons"
        );
    }

    internal static class ChampionIconCache
    {
        private static readonly object Sync = new object();
        private static readonly Dictionary<int, Image> Images =
            new Dictionary<int, Image>();

        public static Image Get(int championId)
        {
            if (championId <= 0)
                return null;
            lock (Sync)
            {
                Image cached;
                if (Images.TryGetValue(championId, out cached))
                    return cached;
                string path = Path.Combine(
                    AppPaths.ChampionIconDirectory,
                    championId.ToString(CultureInfo.InvariantCulture) + ".png"
                );
                if (!File.Exists(path))
                    return null;
                try
                {
                    using (FileStream stream = new FileStream(
                        path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (Image source = Image.FromStream(stream))
                    {
                        cached = new Bitmap(source);
                    }
                    Images[championId] = cached;
                    return cached;
                }
                catch
                {
                    return null;
                }
            }
        }
    }

    public sealed class Repository
    {
        public const string UnassignedAccountKey = "unassigned";
        private readonly object sync = new object();
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
        private readonly string matchesFile;
        private readonly string settingsFile;
        private List<MatchRecord> matches;
        public AppSettings Settings { get; private set; }
        public int LastUpdatedCount { get; private set; }
        public string ActiveAccountKey { get; private set; }

        public Repository(string customDirectory)
        {
            string directory = String.IsNullOrWhiteSpace(customDirectory)
                ? AppPaths.AppDirectory
                : customDirectory;
            Directory.CreateDirectory(directory);
            matchesFile = Path.Combine(directory, "matches.json");
            settingsFile = Path.Combine(directory, "settings.json");
            serializer.MaxJsonLength = Int32.MaxValue;
            matches = Load<List<MatchRecord>>(matchesFile) ?? new List<MatchRecord>();
            foreach (MatchRecord match in matches)
            {
                match.PlayedAt = NormalizeLocalTime(match.PlayedAt);
                if (match.ClaimCheckedAccountKeys == null)
                    match.ClaimCheckedAccountKeys =
                        new List<string>();
            }
            Settings = Load<AppSettings>(settingsFile) ?? new AppSettings();
            if (Settings.Accounts == null)
                Settings.Accounts = new List<AccountProfile>();
            if (Settings.SelectedAccountKey == null)
                Settings.SelectedAccountKey = "";
            ActiveAccountKey = Settings.SelectedAccountKey;
            if (String.IsNullOrWhiteSpace(ActiveAccountKey) &&
                matches.Any(match =>
                    String.IsNullOrWhiteSpace(match.AccountKey)))
                ActiveAccountKey = UnassignedAccountKey;
        }

        public Repository() : this(null) { }

        private T Load<T>(string path) where T : class
        {
            try
            {
                if (!File.Exists(path))
                    return null;
                return serializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch
            {
                string backup = path + ".broken-" +
                    DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                try { File.Copy(path, backup, true); } catch { }
                return null;
            }
        }

        private void AtomicWrite(string path, string content)
        {
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, content, new UTF8Encoding(false));
            if (File.Exists(path))
            {
                string backup = path + ".bak";
                try
                {
                    File.Replace(temporary, path, backup, true);
                    return;
                }
                catch
                {
                    try { File.Delete(path); } catch { }
                }
            }
            File.Move(temporary, path);
        }

        private void SaveMatches()
        {
            AtomicWrite(matchesFile, serializer.Serialize(matches));
        }

        private static DateTime NormalizeLocalTime(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value.ToLocalTime();
            if (value.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(value, DateTimeKind.Local);
            return value;
        }

        public void SaveSettings()
        {
            lock (sync)
            {
                AtomicWrite(settingsFile, serializer.Serialize(Settings));
            }
        }

        private static bool BelongsToAccount(
            MatchRecord match, string accountKey)
        {
            if (String.Equals(
                accountKey, UnassignedAccountKey,
                StringComparison.OrdinalIgnoreCase))
                return String.IsNullOrWhiteSpace(match.AccountKey);
            return String.Equals(
                match.AccountKey, accountKey,
                StringComparison.OrdinalIgnoreCase);
        }

        private IEnumerable<MatchRecord> VisibleMatchesNoLock()
        {
            if (String.IsNullOrWhiteSpace(ActiveAccountKey))
                return Enumerable.Empty<MatchRecord>();
            return matches.Where(
                match => BelongsToAccount(match, ActiveAccountKey));
        }

        public void SetActiveAccount(string accountKey)
        {
            lock (sync)
            {
                ActiveAccountKey = accountKey ?? "";
                Settings.SelectedAccountKey = ActiveAccountKey;
                AtomicWrite(settingsFile, serializer.Serialize(Settings));
            }
        }

        public AccountProfile UpsertAccount(
            string key, string displayName, int profileIconId)
        {
            lock (sync)
            {
                AccountProfile profile = Settings.Accounts.FirstOrDefault(
                    item => String.Equals(
                        item.Key, key, StringComparison.OrdinalIgnoreCase));
                if (profile == null)
                {
                    profile = new AccountProfile();
                    profile.Key = key;
                    Settings.Accounts.Add(profile);
                }
                profile.DisplayName = String.IsNullOrWhiteSpace(displayName)
                    ? profile.DisplayName : displayName;
                profile.ProfileIconId = profileIconId;
                profile.LastSeenAt = DateTime.Now;
                AtomicWrite(settingsFile, serializer.Serialize(Settings));
                return profile;
            }
        }

        public void MarkAccountImported(string key)
        {
            lock (sync)
            {
                AccountProfile profile = Settings.Accounts.FirstOrDefault(
                    item => String.Equals(
                        item.Key, key, StringComparison.OrdinalIgnoreCase));
                if (profile != null)
                {
                    profile.LastImportAt = DateTime.Now;
                    AtomicWrite(settingsFile, serializer.Serialize(Settings));
                }
            }
        }

        public List<AccountProfile> AccountProfiles()
        {
            lock (sync)
            {
                List<AccountProfile> result = Settings.Accounts
                    .OrderByDescending(profile => profile.LastSeenAt)
                    .ToList();
                if (matches.Any(match =>
                    String.IsNullOrWhiteSpace(match.AccountKey)))
                {
                    result.Add(new AccountProfile
                    {
                        Key = UnassignedAccountKey,
                        DisplayName = "未归属数据"
                    });
                }
                return result;
            }
        }

        public int MatchCountForAccount(string accountKey)
        {
            lock (sync)
            {
                return matches.Count(
                    match => BelongsToAccount(match, accountKey));
            }
        }

        public List<string> UnassignedGameIdsForAccount(
            string accountKey, int limit)
        {
            lock (sync)
            {
                return matches
                    .Where(match =>
                        String.IsNullOrWhiteSpace(match.AccountKey) &&
                        String.Equals(
                            match.Source, "auto",
                            StringComparison.OrdinalIgnoreCase) &&
                        !String.IsNullOrWhiteSpace(match.GameId) &&
                        !(match.ClaimCheckedAccountKeys ??
                            new List<string>()).Any(
                                key => String.Equals(
                                    key, accountKey,
                                    StringComparison.OrdinalIgnoreCase)))
                    .OrderByDescending(match => match.PlayedAt)
                    .Take(Math.Max(0, limit))
                    .Select(match => match.GameId)
                    .ToList();
            }
        }

        public int ApplyClaimResults(
            IEnumerable<string> claimedGameIds,
            IEnumerable<string> checkedGameIds,
            string accountKey)
        {
            lock (sync)
            {
                HashSet<string> claimed = new HashSet<string>(
                    claimedGameIds ?? Enumerable.Empty<string>(),
                    StringComparer.OrdinalIgnoreCase);
                HashSet<string> checkedIds = new HashSet<string>(
                    checkedGameIds ?? Enumerable.Empty<string>(),
                    StringComparer.OrdinalIgnoreCase);
                int assigned = 0;
                bool changed = false;
                foreach (MatchRecord match in matches.Where(item =>
                    String.IsNullOrWhiteSpace(item.AccountKey) &&
                    checkedIds.Contains(item.GameId)))
                {
                    if (match.ClaimCheckedAccountKeys == null)
                        match.ClaimCheckedAccountKeys =
                            new List<string>();
                    if (!match.ClaimCheckedAccountKeys.Any(key =>
                        String.Equals(
                            key, accountKey,
                            StringComparison.OrdinalIgnoreCase)))
                    {
                        match.ClaimCheckedAccountKeys.Add(accountKey);
                        changed = true;
                    }
                    if (claimed.Contains(match.GameId))
                    {
                        match.AccountKey = accountKey;
                        assigned++;
                        changed = true;
                    }
                }
                if (changed)
                    SaveMatches();
                return assigned;
            }
        }

        public int Import(
            IEnumerable<MatchRecord> records, string accountKey)
        {
            lock (sync)
            {
                int inserted = 0;
                int updated = 0;
                foreach (MatchRecord record in records)
                {
                    record.PlayedAt = NormalizeLocalTime(record.PlayedAt);
                    if (String.IsNullOrWhiteSpace(record.GameId))
                        continue;
                    MatchRecord existing = matches.FirstOrDefault(match =>
                        String.Equals(
                            match.GameId, record.GameId,
                            StringComparison.OrdinalIgnoreCase) &&
                        (BelongsToAccount(match, accountKey) ||
                         String.IsNullOrWhiteSpace(match.AccountKey)));
                    record.AccountKey = accountKey;
                    if (existing == null)
                    {
                        matches.Add(record);
                        inserted++;
                    }
                    else
                    {
                        bool changed = false;
                        if (String.IsNullOrWhiteSpace(existing.AccountKey))
                        {
                            existing.AccountKey = accountKey;
                            changed = true;
                        }
                        changed |= Enrich(existing, record);
                        if (changed)
                            updated++;
                    }
                }
                LastUpdatedCount = updated;
                if (inserted > 0 || updated > 0)
                    SaveMatches();
                return inserted;
            }
        }

        private static bool Enrich(MatchRecord target, MatchRecord source)
        {
            bool changed = false;
            changed |= Assign(target.DurationSeconds, source.DurationSeconds,
                delegate(int? value) { target.DurationSeconds = value; });
            changed |= Assign(target.Kills, source.Kills,
                delegate(int? value) { target.Kills = value; });
            changed |= Assign(target.Deaths, source.Deaths,
                delegate(int? value) { target.Deaths = value; });
            changed |= Assign(target.Assists, source.Assists,
                delegate(int? value) { target.Assists = value; });
            changed |= Assign(target.ChampionLevel, source.ChampionLevel,
                delegate(int? value) { target.ChampionLevel = value; });
            changed |= Assign(target.GoldEarned, source.GoldEarned,
                delegate(int? value) { target.GoldEarned = value; });
            changed |= Assign(target.DamageToChampions, source.DamageToChampions,
                delegate(int? value) { target.DamageToChampions = value; });
            changed |= Assign(target.DamageTaken, source.DamageTaken,
                delegate(int? value) { target.DamageTaken = value; });
            changed |= Assign(target.TotalHeal, source.TotalHeal,
                delegate(int? value) { target.TotalHeal = value; });
            changed |= Assign(
                target.DamageSelfMitigated,
                source.DamageSelfMitigated,
                delegate(int? value) { target.DamageSelfMitigated = value; });
            if (target.ChampionId == 0 && source.ChampionId != 0)
            {
                target.ChampionId = source.ChampionId;
                changed = true;
            }
            if (String.IsNullOrWhiteSpace(target.GameVersion) &&
                !String.IsNullOrWhiteSpace(source.GameVersion))
            {
                target.GameVersion = source.GameVersion;
                changed = true;
            }
            if (target.QueueId == 0 && source.QueueId != 0)
            {
                target.QueueId = source.QueueId;
                changed = true;
            }
            if (String.IsNullOrWhiteSpace(target.GameMode) &&
                !String.IsNullOrWhiteSpace(source.GameMode))
            {
                target.GameMode = source.GameMode;
                changed = true;
            }
            if (!NamedListsEqual(target.Items, source.Items))
            {
                target.Items = source.Items ?? new List<NamedGameObject>();
                changed = true;
            }
            if (!NamedListsEqual(target.Augments, source.Augments))
            {
                target.Augments = source.Augments ?? new List<NamedGameObject>();
                changed = true;
            }
            if (!target.ParticipantDetailsLoaded &&
                source.ParticipantDetailsLoaded)
            {
                target.ParticipantDetailsLoaded = true;
                target.Teammates =
                    source.Teammates ?? new List<PlayerMatchRecord>();
                target.Opponents =
                    source.Opponents ?? new List<PlayerMatchRecord>();
                changed = true;
            }
            return changed;
        }

        private static bool Assign(
            int? target, int? source, Action<int?> setter)
        {
            if (!source.HasValue || target == source)
                return false;
            setter(source);
            return true;
        }

        private static bool NamedListsEqual(
            List<NamedGameObject> left, List<NamedGameObject> right)
        {
            left = left ?? new List<NamedGameObject>();
            right = right ?? new List<NamedGameObject>();
            if (left.Count != right.Count)
                return false;
            for (int index = 0; index < left.Count; index++)
            {
                if (left[index].Id != right[index].Id ||
                    !String.Equals(
                        left[index].Name,
                        right[index].Name,
                        StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        public void AddManual(string championName, int placement, DateTime playedAt)
        {
            MatchRecord record = new MatchRecord();
            record.AccountKey = ActiveAccountKey;
            record.GameId = "manual-" + Guid.NewGuid().ToString("N");
            record.Source = "manual";
            record.PlayedAt = playedAt;
            record.ChampionName = championName.Trim();
            record.Placement = placement;
            lock (sync)
            {
                matches.Add(record);
                SaveMatches();
            }
        }

        public void Update(
            string gameId, string championName, int placement, DateTime playedAt)
        {
            lock (sync)
            {
                MatchRecord record = matches.FirstOrDefault(
                    x => BelongsToAccount(x, ActiveAccountKey) &&
                        String.Equals(
                            x.GameId, gameId,
                            StringComparison.OrdinalIgnoreCase)
                );
                if (record == null)
                    return;
                record.ChampionName = championName.Trim();
                record.Placement = placement;
                record.PlayedAt = playedAt;
                SaveMatches();
            }
        }

        public void Delete(string gameId)
        {
            lock (sync)
            {
                matches.RemoveAll(
                    x => BelongsToAccount(x, ActiveAccountKey) &&
                        String.Equals(
                            x.GameId, gameId,
                            StringComparison.OrdinalIgnoreCase)
                );
                SaveMatches();
            }
        }

        public List<MatchRecord> Recent(int limit)
        {
            lock (sync)
            {
                return VisibleMatchesNoLock()
                    .OrderByDescending(x => x.PlayedAt)
                    .ThenByDescending(x => x.GameId)
                    .Take(limit).ToList();
            }
        }

        public List<MatchRecord> RecentPage(int offset, int limit)
        {
            lock (sync)
            {
                return VisibleMatchesNoLock()
                    .OrderByDescending(x => x.PlayedAt)
                    .ThenByDescending(x => x.GameId)
                    .Skip(Math.Max(0, offset))
                    .Take(Math.Max(0, limit))
                    .ToList();
            }
        }

        public List<HeroSummary> Summary()
        {
            lock (sync)
            {
                return VisibleMatchesNoLock()
                    .Where(x => !String.IsNullOrWhiteSpace(x.ChampionName))
                    .GroupBy(x => x.ChampionName)
                    .Select(group => new HeroSummary
                    {
                        ChampionId = group
                            .Where(x => x.ChampionId > 0)
                            .Select(x => x.ChampionId)
                            .FirstOrDefault(),
                        ChampionName = group.Key,
                        Picks = group.Count(),
                        Wins = group.Count(x => x.Placement == 1),
                        WinRate = group.Any()
                            ? 100.0 * group.Count(x => x.Placement == 1) / group.Count()
                            : 0,
                        AveragePlacement = group.Average(x => x.Placement)
                    })
                    .OrderByDescending(x => x.Picks)
                    .ThenByDescending(x => x.Wins)
                    .ThenBy(x => x.ChampionName)
                    .ToList();
            }
        }

        public List<string> ChampionNames()
        {
            lock (sync)
            {
                return VisibleMatchesNoLock().Select(x => x.ChampionName)
                    .Where(x => !String.IsNullOrWhiteSpace(x))
                    .Distinct().OrderBy(x => x).ToList();
            }
        }

        public List<string> ParticipantDetailsLoadedGameIds()
        {
            lock (sync)
            {
                return VisibleMatchesNoLock()
                    .Where(x => x.ParticipantDetailsLoaded)
                    .Select(x => x.GameId)
                    .Where(x => !String.IsNullOrWhiteSpace(x))
                    .ToList();
            }
        }

        public List<int> ChampionIds()
        {
            lock (sync)
            {
                return VisibleMatchesNoLock()
                    .SelectMany(match =>
                        new[] { match.ChampionId }
                        .Concat(
                            (match.Teammates ?? new List<PlayerMatchRecord>())
                                .Select(player => player.ChampionId))
                        .Concat(
                            (match.Opponents ?? new List<PlayerMatchRecord>())
                                .Select(player => player.ChampionId)))
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();
            }
        }

        public int MatchCount
        {
            get
            {
                lock (sync)
                {
                    return VisibleMatchesNoLock().Count();
                }
            }
        }
        public int WinCount
        {
            get
            {
                lock (sync)
                {
                    return VisibleMatchesNoLock().Count(
                        x => x.Placement == 1);
                }
            }
        }
    }

    public sealed class ImportResult
    {
        public List<MatchRecord> Matches { get; set; }
        public int HistoryCount { get; set; }
        public string ClientRoot { get; set; }
        public string AccountKey { get; set; }
        public string AccountDisplayName { get; set; }
        public int AccountProfileIconId { get; set; }
        public List<string> ClaimedGameIds { get; set; }
        public List<string> CheckedClaimGameIds { get; set; }

        public ImportResult()
        {
            Matches = new List<MatchRecord>();
            ClientRoot = "";
            AccountKey = "";
            AccountDisplayName = "";
            ClaimedGameIds = new List<string>();
            CheckedClaimGameIds = new List<string>();
        }
    }

    public sealed class BridgeResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public List<MatchRecord> Matches { get; set; }
        public int HistoryCount { get; set; }
        public string ClientRoot { get; set; }
        public string AccountPuuid { get; set; }
        public string AccountDisplayName { get; set; }
        public int AccountProfileIconId { get; set; }
        public List<string> ClaimedGameIds { get; set; }
        public List<string> CheckedClaimGameIds { get; set; }

        public BridgeResponse()
        {
            Error = "";
            Matches = new List<MatchRecord>();
            ClientRoot = "";
            AccountPuuid = "";
            AccountDisplayName = "";
            ClaimedGameIds = new List<string>();
            CheckedClaimGameIds = new List<string>();
        }
    }

    public sealed class ClientAccountInfo
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public int ProfileIconId { get; set; }

        public ClientAccountInfo()
        {
            Key = "";
            DisplayName = "";
        }

        public static string KeyFromPuuid(string puuid)
        {
            if (String.IsNullOrWhiteSpace(puuid))
                return "";
            using (SHA256 algorithm = SHA256.Create())
            {
                byte[] hash = algorithm.ComputeHash(
                    Encoding.UTF8.GetBytes(puuid));
                StringBuilder builder = new StringBuilder();
                for (int index = 0; index < 16; index++)
                    builder.Append(hash[index].ToString(
                        "x2", CultureInfo.InvariantCulture));
                return builder.ToString();
            }
        }
    }

    public sealed class AccountChoice
    {
        public string Key { get; set; }
        public string Label { get; set; }

        public AccountChoice()
        {
            Key = "";
            Label = "";
        }

        public override string ToString()
        {
            return Label;
        }
    }

    public sealed class LcuException : Exception
    {
        public LcuException(string message) : base(message) { }
        public LcuException(string message, Exception inner) : base(message, inner) { }
    }

    public sealed class LcuClient
    {
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
        private readonly int port;
        private readonly string token;
        public string ClientRoot { get; private set; }

        public LcuClient(string preferredRoot)
        {
            serializer.MaxJsonLength = Int32.MaxValue;
            ClientRoot = FindClientRoot(preferredRoot);
            port = 0;
            token = "";
        }

        public ClientAccountInfo ProbeAccount()
        {
            BridgeResponse response = RunAccountProbeBridge();
            return new ClientAccountInfo
            {
                Key = ClientAccountInfo.KeyFromPuuid(
                    response.AccountPuuid),
                DisplayName = response.AccountDisplayName,
                ProfileIconId = response.AccountProfileIconId
            };
        }

        private BridgeResponse RunAccountProbeBridge()
        {
            string baseDirectory =
                AppDomain.CurrentDomain.BaseDirectory;
            string bridgeExe = Path.Combine(
                baseDirectory, "ArenaTrackerBridge.exe");
            string bridgeScript = Path.Combine(
                baseDirectory, "lcu_bridge.py");
            ProcessStartInfo start = new ProcessStartInfo();
            if (File.Exists(bridgeExe))
            {
                start.FileName = bridgeExe;
                start.Arguments = "--account-only --client-root " +
                    Quote(ClientRoot);
            }
            else if (File.Exists(bridgeScript))
            {
                start.FileName = "python";
                start.Arguments = Quote(bridgeScript) +
                    " --account-only --client-root " +
                    Quote(ClientRoot);
            }
            else
            {
                throw new LcuException(
                    "缺少 ArenaTrackerBridge.exe，应用文件可能不完整。");
            }
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.StandardOutputEncoding = Encoding.UTF8;
            start.StandardErrorEncoding = Encoding.UTF8;
            try
            {
                using (Process process = Process.Start(start))
                {
                    string output =
                        process.StandardOutput.ReadToEnd();
                    string error =
                        process.StandardError.ReadToEnd();
                    if (!process.WaitForExit(30000))
                    {
                        try { process.Kill(); } catch { }
                        throw new LcuException(
                            "读取客户端账号超时，请重试。");
                    }
                    if (String.IsNullOrWhiteSpace(output))
                        throw new LcuException(
                            "账号识别组件没有返回数据。" +
                            (String.IsNullOrWhiteSpace(error)
                                ? "" : "\n" + error.Trim()));
                    BridgeResponse response =
                        serializer.Deserialize<BridgeResponse>(output);
                    if (response == null || !response.Success)
                        throw new LcuException(
                            response == null ||
                            String.IsNullOrWhiteSpace(response.Error)
                                ? "无法识别客户端账号。"
                                : response.Error);
                    ClientRoot = response.ClientRoot;
                    return response;
                }
            }
            catch (LcuException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new LcuException(
                    "无法识别当前客户端账号。", exception);
            }
        }

        private static IEnumerable<string> CandidateRoots()
        {
            string[] relatives = new[]
            {
                @"wegame\WeGameApps\英雄联盟\LeagueClient",
                @"WeGameApps\英雄联盟\LeagueClient",
                @"Riot Games\League of Legends",
                @"Program Files\Tencent\英雄联盟\LeagueClient",
                @"Program Files (x86)\Tencent\英雄联盟\LeagueClient"
            };
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady)
                    continue;
                foreach (string relative in relatives)
                    yield return Path.Combine(drive.RootDirectory.FullName, relative);
            }
        }

        private static string FindClientRoot(string preferred)
        {
            List<string> candidates = new List<string>();
            if (!String.IsNullOrWhiteSpace(preferred))
                candidates.Add(preferred);
            candidates.AddRange(CandidateRoots());
            foreach (string root in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    if (File.Exists(Path.Combine(root, "LeagueClient.exe")))
                        return root;
                }
                catch { }
            }
            throw new LcuException(
                "没有找到国服客户端。请先通过 WeGame 登录英雄联盟，" +
                "或点击“客户端目录”手动选择 LeagueClient 文件夹。"
            );
        }

        private string[] ReadConnectionParameters()
        {
            FileInfo log = new DirectoryInfo(ClientRoot)
                .GetFiles("*LeagueClientUx.log")
                .OrderByDescending(x => x.LastWriteTimeUtc)
                .FirstOrDefault();
            if (log == null)
                throw new LcuException("未找到客户端运行日志，请确认客户端已经登录。");
            string content;
            using (FileStream stream = new FileStream(
                log.FullName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete))
            using (StreamReader reader = new StreamReader(
                stream, Encoding.UTF8, true, 4096))
            {
                content = reader.ReadToEnd();
            }
            Match portMatch = Regex.Match(
                content, @"--app-port[= ]+(\d{2,5})", RegexOptions.IgnoreCase
            );
            Match tokenMatch = Regex.Match(
                content, @"--remoting-auth-token[= ]+([^\s]+)",
                RegexOptions.IgnoreCase
            );
            if (!portMatch.Success || !tokenMatch.Success)
                throw new LcuException(
                    "客户端日志中没有连接参数，请重启英雄联盟客户端后重试。"
                );
            return new[]
            {
                portMatch.Groups[1].Value,
                tokenMatch.Groups[1].Value.Trim('"', '\'')
            };
        }

        private object Get(string path)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback =
                delegate { return true; };
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                "https://127.0.0.1:" + port.ToString(CultureInfo.InvariantCulture) + path
            );
            request.Method = "GET";
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;
            request.Accept = "application/json";
            request.UserAgent = "ArenaTrackerCN/0.1";
            request.Headers[HttpRequestHeader.Authorization] =
                "Basic " + Convert.ToBase64String(
                    Encoding.ASCII.GetBytes("riot:" + token)
                );
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(
                    response.GetResponseStream(), Encoding.UTF8))
                {
                    return serializer.DeserializeObject(reader.ReadToEnd());
                }
            }
            catch (Exception exception)
            {
                throw new LcuException(
                    "无法连接英雄联盟客户端，请确认客户端仍在运行并已完成登录。",
                    exception
                );
            }
        }

        private static Dictionary<string, object> Dict(object value)
        {
            return value as Dictionary<string, object>;
        }

        private static object[] Array(object value)
        {
            object[] result = value as object[];
            return result ?? new object[0];
        }

        private static object Value(Dictionary<string, object> data, string key)
        {
            if (data == null || !data.ContainsKey(key))
                return null;
            return data[key];
        }

        private static string Text(Dictionary<string, object> data, string key)
        {
            object value = Value(data, key);
            return value == null ? "" : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static int Number(Dictionary<string, object> data, string key, int fallback)
        {
            object value = Value(data, key);
            if (value == null)
                return fallback;
            int parsed;
            return Int32.TryParse(
                Convert.ToString(value, CultureInfo.InvariantCulture),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out parsed
            ) ? parsed : fallback;
        }

        private static long LongNumber(
            Dictionary<string, object> data, string key, long fallback)
        {
            object value = Value(data, key);
            if (value == null)
                return fallback;
            long parsed;
            return Int64.TryParse(
                Convert.ToString(value, CultureInfo.InvariantCulture),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out parsed
            ) ? parsed : fallback;
        }

        private static bool SamePlayer(
            Dictionary<string, object> current,
            Dictionary<string, object> player)
        {
            foreach (string field in new[] { "summonerId", "puuid", "accountId" })
            {
                string left = Text(current, field);
                string right = Text(player, field);
                if (!String.IsNullOrWhiteSpace(left) &&
                    String.Equals(left, right, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static Dictionary<string, object> OwnParticipant(
            Dictionary<string, object> game,
            Dictionary<string, object> current)
        {
            int participantId = -1;
            foreach (object rawIdentity in Array(Value(game, "participantIdentities")))
            {
                Dictionary<string, object> identity = Dict(rawIdentity);
                Dictionary<string, object> player = Dict(Value(identity, "player"));
                if (SamePlayer(current, player))
                {
                    participantId = Number(identity, "participantId", -1);
                    break;
                }
            }
            if (participantId < 0)
                return null;
            foreach (object rawParticipant in Array(Value(game, "participants")))
            {
                Dictionary<string, object> participant = Dict(rawParticipant);
                if (Number(participant, "participantId", -2) == participantId)
                    return participant;
            }
            return null;
        }

        private static bool IsArena(Dictionary<string, object> game)
        {
            int queueId = Number(game, "queueId", 0);
            string mode = Text(game, "gameMode");
            return queueId == 1700 || queueId == 1710 || queueId == 1750 ||
                String.Equals(mode, "CHERRY", StringComparison.OrdinalIgnoreCase);
        }

        public ImportResult Fetch(
            int historyLimit,
            IEnumerable<string> knownParticipantDetails,
            IEnumerable<int> iconIds = null,
            IEnumerable<string> claimGameIds = null)
        {
            return FetchWithBridge(
                historyLimit, knownParticipantDetails,
                iconIds, claimGameIds);
#pragma warning disable 162
            Dictionary<string, object> current = Dict(
                Get("/lol-summoner/v1/current-summoner")
            );
            Dictionary<string, object> history = Dict(
                Get("/lol-match-history/v1/products/lol/current-summoner/matches" +
                    "?begIndex=0&endIndex=" +
                    historyLimit.ToString(CultureInfo.InvariantCulture))
            );
            object championRaw = Get("/lol-game-data/assets/v1/champion-summary.json");
            object itemRaw = Get("/lol-game-data/assets/v1/items.json");
            object augmentRaw = Get("/lol-game-data/assets/v1/cherry-augments.json");

            Dictionary<int, string> champions = new Dictionary<int, string>();
            foreach (object raw in Array(championRaw))
            {
                Dictionary<string, object> item = Dict(raw);
                int id = Number(item, "id", 0);
                if (id > 0)
                    champions[id] = Text(item, "name");
            }
            Dictionary<int, string> items = new Dictionary<int, string>();
            foreach (object raw in Array(itemRaw))
            {
                Dictionary<string, object> item = Dict(raw);
                int id = Number(item, "id", 0);
                if (id > 0)
                    items[id] = Text(item, "name");
            }
            Dictionary<int, string> augments = new Dictionary<int, string>();
            foreach (object raw in Array(augmentRaw))
            {
                Dictionary<string, object> item = Dict(raw);
                int id = Number(item, "id", 0);
                if (id > 0)
                    augments[id] = Text(item, "nameTRA");
            }

            Dictionary<string, object> gamesContainer = Dict(Value(history, "games"));
            object[] games = Array(Value(gamesContainer, "games"));
            ImportResult result = new ImportResult();
            result.HistoryCount = games.Length;
            result.ClientRoot = ClientRoot;

            foreach (object rawGame in games)
            {
                Dictionary<string, object> game = Dict(rawGame);
                if (!IsArena(game))
                    continue;
                Dictionary<string, object> participant = OwnParticipant(game, current);
                if (participant == null)
                    continue;
                Dictionary<string, object> stats = Dict(Value(participant, "stats"));
                int placement = Number(stats, "subteamPlacement", 0);
                if (placement < 1 || placement > 8)
                    continue;
                int championId = Number(participant, "championId", 0);
                string championName;
                if (!champions.TryGetValue(championId, out championName) ||
                    String.IsNullOrWhiteSpace(championName))
                    championName = "英雄 " + championId.ToString(CultureInfo.InvariantCulture);

                MatchRecord record = new MatchRecord();
                record.GameId = Text(game, "gameId");
                record.Source = "auto";
                long creation = LongNumber(game, "gameCreation", 0);
                record.PlayedAt = creation > 0
                    ? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                        .AddMilliseconds(creation).ToLocalTime()
                    : DateTime.Now;
                record.GameVersion = Text(game, "gameVersion");
                record.QueueId = Number(game, "queueId", 0);
                record.GameMode = Text(game, "gameMode");
                record.ChampionId = championId;
                record.ChampionName = championName;
                record.Placement = placement;
                record.Kills = Number(stats, "kills", 0);
                record.Deaths = Number(stats, "deaths", 0);
                record.Assists = Number(stats, "assists", 0);
                record.ChampionLevel = Number(stats, "champLevel", 0);
                record.GoldEarned = Number(stats, "goldEarned", 0);
                record.DamageToChampions = Number(
                    stats, "totalDamageDealtToChampions", 0);
                record.DamageTaken = Number(stats, "totalDamageTaken", 0);
                record.TotalHeal = Number(stats, "totalHeal", 0);
                record.DamageSelfMitigated = Number(
                    stats, "damageSelfMitigated", 0);

                for (int index = 0; index <= 6; index++)
                {
                    int id = Number(stats, "item" + index, 0);
                    if (id <= 0)
                        continue;
                    string name;
                    if (!items.TryGetValue(id, out name))
                        name = id.ToString(CultureInfo.InvariantCulture);
                    record.Items.Add(new NamedGameObject { Id = id, Name = name });
                }
                for (int index = 1; index <= 6; index++)
                {
                    int id = Number(stats, "playerAugment" + index, 0);
                    if (id <= 0)
                        continue;
                    string name;
                    if (!augments.TryGetValue(id, out name))
                        name = id.ToString(CultureInfo.InvariantCulture);
                    record.Augments.Add(new NamedGameObject { Id = id, Name = name });
                }
                result.Matches.Add(record);
            }
            return result;
#pragma warning restore 162
        }

        private ImportResult FetchWithBridge(
            int historyLimit,
            IEnumerable<string> knownParticipantDetails,
            IEnumerable<int> iconIds,
            IEnumerable<string> claimGameIds)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string bridgeExe = Path.Combine(baseDirectory, "ArenaTrackerBridge.exe");
            string bridgeScript = Path.Combine(baseDirectory, "lcu_bridge.py");
            ProcessStartInfo start = new ProcessStartInfo();
            string knownDetails = String.Join(
                ",",
                (knownParticipantDetails ?? Enumerable.Empty<string>())
                    .Where(x => !String.IsNullOrWhiteSpace(x))
                    .ToArray()
            );
            string requestedIcons = String.Join(
                ",",
                (iconIds ?? Enumerable.Empty<int>())
                    .Where(id => id > 0)
                    .Distinct()
                    .Select(id => id.ToString(CultureInfo.InvariantCulture))
                    .ToArray()
            );
            string iconArguments =
                " --icon-dir " + Quote(AppPaths.ChampionIconDirectory) +
                " --icon-ids " + Quote(requestedIcons);
            string claims = String.Join(
                ",",
                (claimGameIds ?? Enumerable.Empty<string>())
                    .Where(id => !String.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            );
            string claimArguments =
                " --claim-game-ids " + Quote(claims);
            if (File.Exists(bridgeExe))
            {
                start.FileName = bridgeExe;
                start.Arguments =
                    "--client-root " + Quote(ClientRoot) +
                    " --limit " + historyLimit.ToString(CultureInfo.InvariantCulture) +
                    " --known-details " + Quote(knownDetails) +
                    " --detail-limit 12" +
                    iconArguments + claimArguments;
            }
            else if (File.Exists(bridgeScript))
            {
                start.FileName = "python";
                start.Arguments =
                    Quote(bridgeScript) +
                    " --client-root " + Quote(ClientRoot) +
                    " --limit " + historyLimit.ToString(CultureInfo.InvariantCulture) +
                    " --known-details " + Quote(knownDetails) +
                    " --detail-limit 12" +
                    iconArguments + claimArguments;
            }
            else
            {
                throw new LcuException(
                    "缺少 ArenaTrackerBridge.exe，应用文件可能不完整。"
                );
            }
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.StandardOutputEncoding = Encoding.UTF8;
            start.StandardErrorEncoding = Encoding.UTF8;
            try
            {
                using (Process process = Process.Start(start))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    if (!process.WaitForExit(90000))
                    {
                        try { process.Kill(); } catch { }
                        throw new LcuException("读取客户端超时，请重试。");
                    }
                    if (String.IsNullOrWhiteSpace(output))
                        throw new LcuException(
                            "客户端读取组件没有返回数据。" +
                            (String.IsNullOrWhiteSpace(error) ? "" : "\n" + error.Trim())
                        );
                    BridgeResponse response =
                        serializer.Deserialize<BridgeResponse>(output);
                    if (response == null || !response.Success)
                        throw new LcuException(
                            response == null || String.IsNullOrWhiteSpace(response.Error)
                                ? "客户端读取失败。"
                                : response.Error
                        );
                    ClientRoot = response.ClientRoot;
                    return new ImportResult
                    {
                        Matches = response.Matches ?? new List<MatchRecord>(),
                        HistoryCount = response.HistoryCount,
                        ClientRoot = response.ClientRoot,
                        AccountKey = ClientAccountInfo.KeyFromPuuid(
                            response.AccountPuuid),
                        AccountDisplayName = response.AccountDisplayName,
                        AccountProfileIconId =
                            response.AccountProfileIconId,
                        ClaimedGameIds = response.ClaimedGameIds ??
                            new List<string>(),
                        CheckedClaimGameIds =
                            response.CheckedClaimGameIds ??
                            new List<string>()
                    };
                }
            }
            catch (LcuException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new LcuException("无法启动客户端读取组件。", exception);
            }
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? "").Replace("\"", "\\\"") + "\"";
        }
    }

    public sealed class MatchEditDialog : Form
    {
        private readonly ComboBox championBox;
        private readonly NumericUpDown placementBox;
        private readonly DateTimePicker timePicker;
        public string ChampionNameValue { get; private set; }
        public int PlacementValue { get; private set; }
        public DateTime PlayedAtValue { get; private set; }

        public MatchEditDialog(
            IEnumerable<string> champions, MatchRecord existing)
        {
            Text = existing == null ? "手动录入" : "编辑对局";
            ClientSize = new Size(430, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.Panel;
            ForeColor = Theme.Text;
            Font = Theme.Font(9.5f, FontStyle.Regular);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(24);
            layout.RowCount = 5;
            layout.ColumnCount = 2;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            championBox = new ComboBox();
            championBox.DropDownStyle = ComboBoxStyle.DropDown;
            championBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            championBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            championBox.Dock = DockStyle.Fill;
            foreach (string champion in champions)
                championBox.Items.Add(champion);
            championBox.Text = existing == null ? "" : existing.ChampionName;

            placementBox = new NumericUpDown();
            placementBox.Minimum = 1;
            placementBox.Maximum = 8;
            placementBox.Value = existing == null ? 1 : existing.Placement;
            placementBox.Width = 100;

            timePicker = new DateTimePicker();
            timePicker.CustomFormat = "yyyy-MM-dd HH:mm";
            timePicker.Format = DateTimePickerFormat.Custom;
            timePicker.Value = existing == null ? DateTime.Now : existing.PlayedAt;
            timePicker.Width = 220;

            AddRow(layout, 0, "英雄", championBox);
            AddRow(layout, 1, "最终名次", placementBox);
            AddRow(layout, 2, "对局时间", timePicker);

            Label hint = new Label();
            hint.Text = "保存完整名次后可统计平均名次；第 1 名计为吃鸡。";
            hint.ForeColor = Theme.Muted;
            hint.AutoSize = true;
            hint.Margin = new Padding(0, 8, 0, 0);
            layout.Controls.Add(hint, 1, 3);

            FlowLayoutPanel actions = new FlowLayoutPanel();
            actions.FlowDirection = FlowDirection.RightToLeft;
            actions.Dock = DockStyle.Fill;
            actions.Margin = new Padding(0, 15, 0, 0);
            Button save = Theme.Button("保存", Theme.Accent, Color.FromArgb(17, 24, 39));
            Button cancel = Theme.Button("取消", Theme.Border, Theme.Text);
            save.Click += delegate { SaveAndClose(); };
            cancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            actions.Controls.Add(save);
            actions.Controls.Add(cancel);
            layout.Controls.Add(actions, 0, 4);
            layout.SetColumnSpan(actions, 2);
            AcceptButton = save;
            CancelButton = cancel;
        }

        private static void AddRow(
            TableLayoutPanel layout, int row, string labelText, Control control)
        {
            Label label = new Label();
            label.Text = labelText;
            label.ForeColor = Theme.Text;
            label.AutoSize = true;
            label.Anchor = AnchorStyles.Left;
            label.Margin = new Padding(0, 10, 8, 8);
            control.Margin = new Padding(0, 7, 0, 7);
            layout.Controls.Add(label, 0, row);
            layout.Controls.Add(control, 1, row);
        }

        private void SaveAndClose()
        {
            string champion = championBox.Text.Trim();
            if (String.IsNullOrWhiteSpace(champion))
            {
                MessageBox.Show(this, "请输入或选择英雄。", "缺少英雄",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ChampionNameValue = champion;
            PlacementValue = Decimal.ToInt32(placementBox.Value);
            PlayedAtValue = timePicker.Value;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public sealed class MatchDetailsDialog : Form
    {
        public MatchDetailsDialog(
            MatchRecord record, bool privacyMode = false)
        {
            Text = "对局详情";
            ClientSize = new Size(900, 680);
            MinimumSize = new Size(780, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            Font = Theme.Font(9.5f, FontStyle.Regular);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(22);
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.BackColor = Theme.Background;
            Controls.Add(root);

            Panel header = new Panel();
            header.Dock = DockStyle.Fill;
            header.BackColor = Theme.Panel;
            Image championIcon = ChampionIconCache.Get(record.ChampionId);
            int headerTextLeft = championIcon == null ? 16 : 76;
            if (championIcon != null)
            {
                PictureBox icon = new PictureBox();
                icon.Image = championIcon;
                icon.SizeMode = PictureBoxSizeMode.Zoom;
                icon.Location = new Point(14, 10);
                icon.Size = new Size(48, 48);
                header.Controls.Add(icon);
            }
            Label title = new Label();
            title.Text = record.ChampionName + "  ·  第 " +
                record.Placement.ToString(CultureInfo.InvariantCulture) + " 名";
            title.Font = Theme.Font(17, FontStyle.Bold);
            title.ForeColor = record.Placement == 1
                ? Color.FromArgb(247, 202, 105)
                : Theme.Text;
            title.AutoSize = true;
            title.Location = new Point(headerTextLeft, 12);
            Label subtitle = new Label();
            subtitle.Text = record.PlayedAt.ToString("yyyy-MM-dd HH:mm") +
                "   ·   " + FormatDuration(record.DurationSeconds) +
                "   ·   " + (String.IsNullOrWhiteSpace(record.GameVersion)
                    ? "版本未知" : "版本 " + record.GameVersion);
            subtitle.ForeColor = Theme.Muted;
            subtitle.AutoSize = true;
            subtitle.Location = new Point(headerTextLeft + 2, 43);
            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            root.Controls.Add(header, 0, 0);

            List<PlayerMatchRecord> teammates =
                record.Teammates ?? new List<PlayerMatchRecord>();
            List<PlayerMatchRecord> opponents =
                record.Opponents ?? new List<PlayerMatchRecord>();
            List<Tuple<string, Control>> sections =
                new List<Tuple<string, Control>>();
            sections.Add(Tuple.Create("本人数据", CreateSelfPanel(record)));
            sections.Add(Tuple.Create(
                record.ParticipantDetailsLoaded
                    ? "队友 (" + teammates.Count.ToString(
                        CultureInfo.InvariantCulture) + ")"
                    : "队友",
                CreatePlayersPanel(
                    teammates, record.ParticipantDetailsLoaded,
                    "队友", privacyMode)));
            sections.Add(Tuple.Create(
                record.ParticipantDetailsLoaded
                    ? "对手 (" + opponents.Count.ToString(
                        CultureInfo.InvariantCulture) + ")"
                    : "对手",
                CreatePlayersPanel(
                    opponents, record.ParticipantDetailsLoaded,
                    "对手", privacyMode)));
            root.Controls.Add(CreateSectionSwitcher(sections), 0, 1);

            FlowLayoutPanel footer = new FlowLayoutPanel();
            footer.Dock = DockStyle.Fill;
            footer.FlowDirection = FlowDirection.RightToLeft;
            footer.Padding = new Padding(0, 8, 0, 0);
            Button close = Theme.Button("关闭", Theme.Border, Theme.Text);
            close.Click += delegate { Close(); };
            footer.Controls.Add(close);
            Label gameId = new Label();
            gameId.Text = "对局 ID：" + record.GameId;
            gameId.ForeColor = Theme.Muted;
            gameId.AutoSize = true;
            gameId.Margin = new Padding(0, 10, 20, 0);
            footer.Controls.Add(gameId);
            root.Controls.Add(footer, 0, 2);
            AcceptButton = close;
            CancelButton = close;
        }

        private static Control CreateSectionSwitcher(
            List<Tuple<string, Control>> sections)
        {
            Panel host = new Panel();
            host.Dock = DockStyle.Fill;
            host.Margin = new Padding(0, 10, 0, 0);
            host.BackColor = Theme.Background;
            FlowLayoutPanel navigation = new FlowLayoutPanel();
            navigation.Dock = DockStyle.Top;
            navigation.Height = 38;
            navigation.WrapContents = false;
            navigation.FlowDirection = FlowDirection.LeftToRight;
            navigation.Padding = new Padding(0);
            navigation.Margin = new Padding(0);
            navigation.BackColor = Theme.Background;
            Panel body = new Panel();
            body.Dock = DockStyle.Fill;
            body.Padding = new Padding(0, 8, 0, 0);
            body.BackColor = Theme.Background;
            host.Controls.Add(body);
            host.Controls.Add(navigation);

            List<Button> buttons = new List<Button>();
            List<Control> contents = new List<Control>();
            Action<int> select = delegate(int selectedIndex)
            {
                for (int index = 0; index < contents.Count; index++)
                {
                    bool selected = index == selectedIndex;
                    contents[index].Visible = selected;
                    if (selected)
                        contents[index].BringToFront();
                    buttons[index].BackColor =
                        selected ? Theme.Panel : Theme.PanelAlt;
                    buttons[index].ForeColor =
                        selected ? Theme.Accent : Theme.Muted;
                }
            };
            for (int index = 0; index < sections.Count; index++)
            {
                int capturedIndex = index;
                Button button = new Button();
                button.Text = sections[index].Item1;
                button.Size = new Size(145, 36);
                button.Margin = new Padding(0, 0, 2, 0);
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Theme.Border;
                button.FlatAppearance.BorderSize = 1;
                button.Font = Theme.Font(9.5f, FontStyle.Bold);
                button.Cursor = Cursors.Hand;
                button.Click += delegate { select(capturedIndex); };
                buttons.Add(button);
                navigation.Controls.Add(button);

                Control content = sections[index].Item2;
                content.Dock = DockStyle.Fill;
                contents.Add(content);
                body.Controls.Add(content);
            }
            select(0);
            return host;
        }

        private static Control CreateSelfPanel(MatchRecord record)
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Theme.Background;
            layout.Padding = new Padding(0, 8, 0, 0);
            layout.ColumnCount = 1;
            layout.RowCount = 3;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 185));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            TableLayoutPanel stats = new TableLayoutPanel();
            stats.Dock = DockStyle.Fill;
            stats.BackColor = Theme.Panel;
            stats.Margin = new Padding(0);
            stats.Padding = new Padding(12);
            stats.ColumnCount = 4;
            stats.RowCount = 2;
            for (int column = 0; column < 4; column++)
                stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            stats.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            stats.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            AddStat(stats, 0, 0, "K / D / A", FormatKda(record));
            AddStat(stats, 1, 0, "获得金币", FormatNumber(record.GoldEarned));
            AddStat(stats, 2, 0, "对英雄伤害", FormatNumber(record.DamageToChampions));
            AddStat(stats, 3, 0, "承受伤害", FormatNumber(record.DamageTaken));
            AddStat(stats, 0, 1, "治疗量", FormatNumber(record.TotalHeal));
            AddStat(stats, 1, 1, "自我减伤", FormatNumber(record.DamageSelfMitigated));
            AddStat(stats, 2, 1, "英雄等级", FormatNumber(record.ChampionLevel));
            AddStat(stats, 3, 1, "队列 / 模式",
                record.QueueId.ToString(CultureInfo.InvariantCulture) +
                " · " + (String.IsNullOrWhiteSpace(record.GameMode)
                    ? "—" : record.GameMode));
            layout.Controls.Add(stats, 0, 0);
            layout.Controls.Add(
                CollectionPanel("强化符文", record.Augments), 0, 1);
            layout.Controls.Add(
                CollectionPanel("装备信息", record.Items), 0, 2);
            return layout;
        }

        private static Control CreatePlayersPanel(
            List<PlayerMatchRecord> players,
            bool detailsLoaded,
            string relationship,
            bool privacyMode)
        {
            Panel container = new Panel();
            container.Dock = DockStyle.Fill;
            container.BackColor = Theme.Background;
            container.Padding = new Padding(0, 8, 0, 0);
            if (!detailsLoaded)
            {
                Label unavailable = new Label();
                unavailable.Dock = DockStyle.Fill;
                unavailable.BackColor = Theme.Panel;
                unavailable.ForeColor = Theme.Muted;
                unavailable.Font = Theme.Font(10, FontStyle.Regular);
                unavailable.TextAlign = ContentAlignment.MiddleCenter;
                unavailable.Text =
                    "该场阵容尚未补全。\r\n保持客户端在线，后续自动导入会分批补齐历史对局。";
                container.Controls.Add(unavailable);
                return container;
            }
            if (players == null || players.Count == 0)
            {
                Label empty = new Label();
                empty.Dock = DockStyle.Fill;
                empty.BackColor = Theme.Panel;
                empty.ForeColor = Theme.Muted;
                empty.TextAlign = ContentAlignment.MiddleCenter;
                empty.Text = "该场没有可显示的" + relationship + "数据。";
                container.Controls.Add(empty);
                return container;
            }

            TableLayoutPanel split = new TableLayoutPanel();
            split.Dock = DockStyle.Fill;
            split.ColumnCount = 1;
            split.RowCount = 2;
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            split.BackColor = Theme.Background;
            container.Controls.Add(split);

            DataGridView grid = CreatePlayerGrid();
            List<IGrouping<string, PlayerMatchRecord>> teams = players
                .OrderBy(player => player.Placement)
                .ThenBy(player => player.SubteamId)
                .GroupBy(
                    player => player.SubteamId != 0
                        ? "team-" + player.SubteamId.ToString(
                            CultureInfo.InvariantCulture)
                        : "placement-" + player.Placement.ToString(
                            CultureInfo.InvariantCulture))
                .ToList();
            Dictionary<PlayerMatchRecord, string> displayNames =
                new Dictionary<PlayerMatchRecord, string>();
            int playerNumber = 0;
            for (int teamIndex = 0; teamIndex < teams.Count; teamIndex++)
            {
                List<PlayerMatchRecord> team = teams[teamIndex].ToList();
                for (int memberIndex = 0;
                    memberIndex < team.Count; memberIndex++)
                {
                    PlayerMatchRecord player = team[memberIndex];
                    playerNumber++;
                    string displayName = privacyMode
                        ? relationship + " " + playerNumber.ToString(
                            CultureInfo.InvariantCulture)
                        : player.RiotId;
                    displayNames[player] = displayName;
                    int index = grid.Rows.Add(
                        ChampionIconCache.Get(player.ChampionId),
                        displayName,
                        player.ChampionName,
                        player.Placement > 0
                            ? "第 " + player.Placement.ToString(
                                CultureInfo.InvariantCulture) + " 名"
                            : "—",
                        FormatKda(player),
                        FormatNumber(player.DamageToChampions),
                        FormatNumber(player.DamageTaken)
                    );
                    DataGridViewRow row = grid.Rows[index];
                    row.Tag = player;
                    if (teamIndex % 2 == 1)
                        row.DefaultCellStyle.BackColor =
                            Color.FromArgb(20, 31, 51);
                    if (memberIndex == team.Count - 1 &&
                        teamIndex < teams.Count - 1)
                        row.DividerHeight = 6;
                }
            }
            split.Controls.Add(grid, 0, 0);

            Panel detail = new Panel();
            detail.Dock = DockStyle.Fill;
            detail.BackColor = Theme.Panel;
            detail.Margin = new Padding(0, 10, 0, 0);
            detail.Padding = new Padding(16, 12, 16, 10);
            Label selectedTitle = new Label();
            selectedTitle.ForeColor = Theme.Text;
            selectedTitle.Font = Theme.Font(11, FontStyle.Bold);
            selectedTitle.AutoEllipsis = true;
            selectedTitle.AutoSize = false;
            selectedTitle.Dock = DockStyle.Top;
            selectedTitle.Height = 26;
            Label summary = new Label();
            summary.ForeColor = Theme.Muted;
            summary.AutoEllipsis = true;
            summary.AutoSize = false;
            summary.Dock = DockStyle.Top;
            summary.Height = 26;
            Label augments = DetailLine();
            Label items = DetailLine();
            items.Dock = DockStyle.Fill;
            detail.Controls.Add(items);
            detail.Controls.Add(augments);
            detail.Controls.Add(summary);
            detail.Controls.Add(selectedTitle);
            split.Controls.Add(detail, 0, 1);

            Action updateSelection = delegate
            {
                if (grid.SelectedRows.Count == 0)
                    return;
                PlayerMatchRecord player =
                    grid.SelectedRows[0].Tag as PlayerMatchRecord;
                if (player == null)
                    return;
                string displayName;
                if (!displayNames.TryGetValue(player, out displayName))
                    displayName = privacyMode ? relationship : player.RiotId;
                selectedTitle.Text = displayName + "  ·  " +
                    player.ChampionName;
                summary.Text = "KDA " + FormatKda(player) +
                    "    金币 " + FormatNumber(player.GoldEarned) +
                    "    英雄伤害 " + FormatNumber(player.DamageToChampions) +
                    "    承伤 " + FormatNumber(player.DamageTaken) +
                    "    治疗 " + FormatNumber(player.TotalHeal) +
                    "    自我减伤 " + FormatNumber(
                        player.DamageSelfMitigated);
                augments.Text = "强化：" + FormatObjects(player.Augments);
                items.Text = "装备：" + FormatObjects(player.Items);
            };
            grid.SelectionChanged += delegate { updateSelection(); };
            if (grid.Rows.Count > 0)
            {
                grid.Rows[0].Selected = true;
                updateSelection();
            }
            return container;
        }

        private static Label DetailLine()
        {
            Label label = new Label();
            label.ForeColor = Theme.Text;
            label.Font = Theme.Font(9.5f, FontStyle.Regular);
            label.AutoEllipsis = true;
            label.AutoSize = false;
            label.Dock = DockStyle.Top;
            label.Height = 34;
            label.Padding = new Padding(0, 5, 0, 0);
            return label;
        }

        private static DataGridView CreatePlayerGrid()
        {
            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Theme.Panel;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Theme.Border;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.PanelAlt;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Muted;
            grid.ColumnHeadersDefaultCellStyle.Font =
                Theme.Font(9, FontStyle.Bold);
            grid.ColumnHeadersHeight = 36;
            grid.RowHeadersVisible = false;
            grid.RowTemplate.Height = 42;
            grid.DefaultCellStyle.BackColor = Theme.Panel;
            grid.DefaultCellStyle.ForeColor = Theme.Text;
            grid.DefaultCellStyle.SelectionBackColor = Theme.Selection;
            grid.DefaultCellStyle.SelectionForeColor = Theme.Text;
            grid.DefaultCellStyle.Font = Theme.Font(9, FontStyle.Regular);
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DataGridViewImageColumn iconColumn =
                new DataGridViewImageColumn();
            iconColumn.Name = "Icon";
            iconColumn.HeaderText = "";
            iconColumn.MinimumWidth = 42;
            iconColumn.Width = 42;
            iconColumn.FillWeight = 38;
            iconColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            iconColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            iconColumn.DefaultCellStyle.NullValue = null;
            iconColumn.DefaultCellStyle.Padding = new Padding(4);
            grid.Columns.Add(iconColumn);
            grid.Columns.Add("RiotId", "玩家");
            grid.Columns.Add("Champion", "英雄");
            grid.Columns.Add("Placement", "名次");
            grid.Columns.Add("Kda", "K / D / A");
            grid.Columns.Add("Damage", "英雄伤害");
            grid.Columns.Add("Taken", "承受伤害");
            grid.Columns[0].FillWeight = 38;
            grid.Columns[1].FillWeight = 145;
            grid.Columns[2].FillWeight = 85;
            grid.Columns[3].FillWeight = 62;
            grid.Columns[4].FillWeight = 82;
            grid.Columns[5].FillWeight = 82;
            grid.Columns[6].FillWeight = 82;
            return grid;
        }

        private static void AddStat(
            TableLayoutPanel panel, int column, int row, string name, string value)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = Theme.PanelAlt;
            card.Margin = new Padding(4);
            Label nameLabel = new Label();
            nameLabel.Text = name;
            nameLabel.ForeColor = Theme.Muted;
            nameLabel.Font = Theme.Font(8.5f, FontStyle.Regular);
            nameLabel.AutoSize = true;
            nameLabel.Location = new Point(10, 9);
            Label valueLabel = new Label();
            valueLabel.Text = value;
            valueLabel.ForeColor = Theme.Text;
            valueLabel.Font = Theme.Font(12, FontStyle.Bold);
            valueLabel.AutoEllipsis = true;
            valueLabel.AutoSize = false;
            valueLabel.Location = new Point(9, 31);
            valueLabel.Size = new Size(140, 28);
            valueLabel.Anchor = AnchorStyles.Top |
                AnchorStyles.Left | AnchorStyles.Right;
            card.Controls.Add(nameLabel);
            card.Controls.Add(valueLabel);
            panel.Controls.Add(card, column, row);
        }

        private static Panel CollectionPanel(
            string titleText, List<NamedGameObject> values)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Theme.Panel;
            panel.Margin = new Padding(0, 10, 0, 0);
            Label title = new Label();
            title.Text = titleText;
            title.ForeColor = Theme.Text;
            title.Font = Theme.Font(10.5f, FontStyle.Bold);
            title.AutoSize = true;
            title.Location = new Point(14, 11);
            Label content = new Label();
            List<NamedGameObject> safeValues =
                values ?? new List<NamedGameObject>();
            content.Text = safeValues.Count == 0
                ? "暂无数据"
                : String.Join(
                    "    ",
                    safeValues.Select(
                        item => String.IsNullOrWhiteSpace(item.Name)
                            ? item.Id.ToString(CultureInfo.InvariantCulture)
                            : item.Name).ToArray());
            content.ForeColor = safeValues.Count == 0 ? Theme.Muted : Theme.Text;
            content.Font = Theme.Font(10, FontStyle.Regular);
            content.AutoEllipsis = true;
            content.AutoSize = false;
            content.Location = new Point(14, 42);
            content.Size = new Size(640, 55);
            content.Anchor = AnchorStyles.Top |
                AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            panel.Controls.Add(title);
            panel.Controls.Add(content);
            return panel;
        }

        private static string FormatDuration(int? seconds)
        {
            if (!seconds.HasValue || seconds.Value <= 0)
                return "时长未知";
            return (seconds.Value / 60).ToString(CultureInfo.InvariantCulture) +
                "分" + (seconds.Value % 60).ToString("00",
                    CultureInfo.InvariantCulture) + "秒";
        }

        private static string FormatKda(MatchRecord record)
        {
            if (!record.Kills.HasValue || !record.Deaths.HasValue ||
                !record.Assists.HasValue)
                return "—";
            return record.Kills.Value + " / " +
                record.Deaths.Value + " / " + record.Assists.Value;
        }

        private static string FormatKda(PlayerMatchRecord record)
        {
            if (!record.Kills.HasValue || !record.Deaths.HasValue ||
                !record.Assists.HasValue)
                return "—";
            return record.Kills.Value + " / " +
                record.Deaths.Value + " / " + record.Assists.Value;
        }

        private static string FormatObjects(List<NamedGameObject> values)
        {
            List<NamedGameObject> safe =
                values ?? new List<NamedGameObject>();
            if (safe.Count == 0)
                return "暂无数据";
            return String.Join(
                "、",
                safe.Select(
                    item => String.IsNullOrWhiteSpace(item.Name)
                        ? item.Id.ToString(CultureInfo.InvariantCulture)
                        : item.Name).ToArray()
            );
        }

        private static string FormatNumber(int? value)
        {
            return value.HasValue
                ? value.Value.ToString("N0", CultureInfo.InvariantCulture)
                : "—";
        }
    }

    public sealed class StatusDot : Label
    {
        private Color stateColor = Theme.Muted;

        public Color StateColor
        {
            get { return stateColor; }
            set
            {
                stateColor = value;
                ForeColor = value;
            }
        }

        public StatusDot()
        {
            Text = "●";
            ForeColor = stateColor;
            BackColor = Theme.PanelAlt;
            Font = Theme.Font(11, FontStyle.Bold);
            TextAlign = ContentAlignment.MiddleCenter;
            AutoSize = false;
            Size = new Size(20, 28);
        }
    }

    public sealed class PlacementTrendControl : Control
    {
        private List<MatchRecord> records = new List<MatchRecord>();

        public PlacementTrendControl()
        {
            BackColor = Theme.PanelAlt;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true
            );
        }

        public void SetData(IEnumerable<MatchRecord> source)
        {
            records = (source ?? Enumerable.Empty<MatchRecord>())
                .Where(x => x.Placement >= 1 && x.Placement <= 8)
                .OrderBy(x => x.PlayedAt)
                .ToList();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Theme.PanelAlt);

            if (records.Count == 0)
            {
                using (Font font = Theme.Font(9.5f, FontStyle.Regular))
                using (SolidBrush brush = new SolidBrush(Theme.Muted))
                {
                    string empty = "暂无名次数据";
                    SizeF size = graphics.MeasureString(empty, font);
                    graphics.DrawString(
                        empty, font, brush,
                        (Width - size.Width) / 2,
                        (Height - size.Height) / 2
                    );
                }
                return;
            }

            Rectangle plot = new Rectangle(
                14, 14, Math.Max(10, Width - 28), Math.Max(10, Height - 40));
            using (Pen gridPen = new Pen(Color.FromArgb(46, Theme.Border), 1))
            using (Font rankFont = Theme.Font(8, FontStyle.Regular))
            using (SolidBrush mutedBrush = new SolidBrush(Theme.Muted))
            {
                for (int rank = 1; rank <= 8; rank++)
                {
                    float y = RankY(plot, rank);
                    graphics.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                }
                graphics.DrawString(
                    records.First().PlayedAt.ToString("MM-dd"),
                    rankFont, mutedBrush, plot.Left, plot.Bottom + 6
                );
                string lastDate = records.Last().PlayedAt.ToString("MM-dd");
                SizeF lastSize = graphics.MeasureString(lastDate, rankFont);
                graphics.DrawString(
                    lastDate, rankFont, mutedBrush,
                    plot.Right - lastSize.Width, plot.Bottom + 6
                );
            }

            PointF[] points = new PointF[records.Count];
            for (int index = 0; index < records.Count; index++)
            {
                float x = records.Count == 1
                    ? plot.Left + plot.Width / 2f
                    : plot.Left + plot.Width * index / (records.Count - 1f);
                points[index] = new PointF(x, RankY(plot, records[index].Placement));
            }

            if (points.Length > 1)
            {
                using (Pen linePen = new Pen(
                    Color.FromArgb(102, 170, 255), 2.2f))
                {
                    linePen.LineJoin = LineJoin.Round;
                    graphics.DrawLines(linePen, points);
                }
            }

            for (int index = 0; index < points.Length; index++)
            {
                Color pointColor = records[index].Placement == 1
                    ? Theme.Accent : Color.FromArgb(102, 170, 255);
                using (SolidBrush pointBrush = new SolidBrush(pointColor))
                    graphics.FillEllipse(
                        pointBrush, points[index].X - 3.5f,
                        points[index].Y - 3.5f, 7, 7
                    );
                using (Pen edge = new Pen(Theme.PanelAlt, 1.2f))
                    graphics.DrawEllipse(
                        edge, points[index].X - 3.5f,
                        points[index].Y - 3.5f, 7, 7
                    );

                using (Font labelFont = Theme.Font(8, FontStyle.Bold))
                using (SolidBrush labelBrush = new SolidBrush(pointColor))
                {
                    string label = "第" +
                        records[index].Placement.ToString(
                            CultureInfo.InvariantCulture);
                    SizeF labelSize = graphics.MeasureString(label, labelFont);
                    float labelY = records[index].Placement <= 2
                        ? points[index].Y + 5
                        : points[index].Y - labelSize.Height - 3;
                    graphics.DrawString(
                        label, labelFont, labelBrush,
                        points[index].X - labelSize.Width / 2f, labelY
                    );
                }
            }
        }

        private static float RankY(Rectangle plot, int rank)
        {
            return plot.Top + (rank - 1) * plot.Height / 7f;
        }
    }

    public sealed class UsageNoticeDialog : Form
    {
        private const string AuthorUrl = "https://b23.tv/6NV8zm6";

        public UsageNoticeDialog()
        {
            Text = "使用须知";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(680, 590);
            MinimumSize = new Size(620, 520);
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            Font = Theme.Font(9.5f, FontStyle.Regular);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(24, 20, 24, 18);
            layout.ColumnCount = 1;
            layout.RowCount = 3;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            Controls.Add(layout);

            Label title = new Label();
            title.Text = "使用须知";
            title.Font = Theme.Font(20, FontStyle.Bold);
            title.ForeColor = Theme.Text;
            title.Dock = DockStyle.Fill;
            title.TextAlign = ContentAlignment.MiddleLeft;
            layout.Controls.Add(title, 0, 0);

            FlowLayoutPanel noticePanel = new FlowLayoutPanel();
            noticePanel.Dock = DockStyle.Fill;
            noticePanel.AutoScroll = true;
            noticePanel.FlowDirection = FlowDirection.TopDown;
            noticePanel.WrapContents = false;
            noticePanel.TabStop = false;
            noticePanel.BackColor = Theme.Panel;
            noticePanel.Padding = new Padding(16, 14, 12, 14);
            noticePanel.Margin = new Padding(0, 8, 0, 8);

            Label notice = new Label();
            notice.AutoSize = true;
            notice.ForeColor = Theme.Text;
            notice.Font = Theme.Font(10, FontStyle.Regular);
            notice.Margin = new Padding(0);
            notice.Text =
                "一、功能介绍与注意事项\n\n" +
                "1. 本软件用于读取英雄联盟国服客户端提供的赛后数据，统计斗魂竞技场个人对局、英雄选取、第一名次数、名次走势及对局详情。\n\n" +
                "2. 数据默认只保存在本机。自动导入时请保持客户端已登录，并切换到与当前登录账号一致的账号页面。\n\n" +
                "3. 本软件不读取游戏内存、不注入游戏进程、不修改客户端，也不保存客户端临时认证令牌。客户端接口、游戏版本或服务器状态变化可能导致部分功能暂时不可用。\n\n" +
                "4. 建议定期备份 %LOCALAPPDATA%\\ArenaTrackerCN。使用多个游戏客户端实例时，应确认当前连接账号后再导入。\n\n" +
                "二、使用规则与法律免责声明\n\n" +
                "1. 本软件仅授权个人、非商业用途使用。未经作者明确书面授权，不允许私自商用化，包括但不限于销售、收费分发、付费捆绑、广告变现或以本软件为基础提供收费服务。\n\n" +
                "2. 未经作者许可，不得删除或篡改作者署名、冒充官方版本，或以可能造成误解的方式进行再发布。\n\n" +
                "3. 英雄联盟、游戏名称、美术素材及相关商标的权利归其各自权利人所有。本软件为独立个人工具，与游戏运营方不存在官方隶属、授权或担保关系。\n\n" +
                "4. 用户应自行遵守适用法律、游戏用户协议和平台规则，不得将本软件用于作弊、侵害他人权益或其他违法违规用途。\n\n" +
                "5. 软件按现状提供，不保证数据永久可得、完全准确或适用于特定目的。因客户端变更、服务中断、数据丢失或不当使用产生的风险由使用者自行承担；法律另有强制规定的除外。\n\n" +
                "6. 本说明是软件使用规则和一般风险提示，不构成法律意见。具体权利义务以适用法律及作者另行出具的授权文件为准。\n\n" +
                "三、作者信息\n\n" +
                "作者：并州司马锦 & 笛非竹\nB站链接：";
            noticePanel.Controls.Add(notice);

            LinkLabel authorLink = new LinkLabel();
            authorLink.Text = AuthorUrl;
            authorLink.AutoSize = true;
            authorLink.TabStop = false;
            authorLink.LinkColor = Theme.Accent;
            authorLink.ActiveLinkColor = Theme.AccentHover;
            authorLink.VisitedLinkColor = Theme.Accent;
            authorLink.Font = Theme.Font(10, FontStyle.Underline);
            authorLink.Margin = new Padding(0, 2, 0, 12);
            authorLink.LinkClicked += delegate(
                object sender, LinkLabelLinkClickedEventArgs eventArgs)
            {
                try
                {
                    Process.Start(AuthorUrl);
                }
                catch
                {
                    MessageBox.Show(
                        this,
                        "无法打开链接，请复制到浏览器访问：\n" +
                        AuthorUrl,
                        "无法打开浏览器",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            };
            noticePanel.Controls.Add(authorLink);
            Action sizeNotice = delegate
            {
                int width = Math.Max(
                    120,
                    noticePanel.ClientSize.Width -
                    noticePanel.Padding.Horizontal -
                    SystemInformation.VerticalScrollBarWidth - 6);
                notice.MaximumSize = new Size(width, 0);
                authorLink.MaximumSize = new Size(width, 0);
            };
            noticePanel.SizeChanged += delegate { sizeNotice(); };
            sizeNotice();
            layout.Controls.Add(noticePanel, 0, 1);

            FlowLayoutPanel actions = new FlowLayoutPanel();
            actions.Dock = DockStyle.Fill;
            actions.FlowDirection = FlowDirection.RightToLeft;
            actions.Padding = new Padding(0, 8, 0, 0);
            Button close = Theme.Button(
                "我已了解", Theme.Accent,
                Color.FromArgb(17, 24, 39));
            close.Click += delegate { Close(); };
            actions.Controls.Add(close);
            layout.Controls.Add(actions, 0, 2);
            Shown += delegate
            {
                noticePanel.AutoScrollPosition = Point.Empty;
                close.Focus();
            };
        }
    }

    public sealed class MainForm : Form
    {
        private const int RecentPageSize = 50;
        private readonly Repository repository;
        private readonly Label matchesValue;
        private readonly Label winsValue;
        private readonly Label rateValue;
        private readonly Label favoriteValue;
        private readonly DataGridView summaryGrid;
        private readonly DataGridView recentGrid;
        private readonly TextBox searchBox;
        private readonly Label statusLabel;
        private readonly StatusDot connectionDot;
        private readonly Label streakValue;
        private readonly Label todayValue;
        private readonly Label weekValue;
        private readonly PlacementTrendControl trendChart;
        private readonly Button importButton;
        private readonly Button accountButton;
        private readonly ContextMenuStrip accountMenu;
        private readonly List<AccountChoice> accountChoices;
        private readonly Button recentFirstButton;
        private readonly Button recentPreviousButton;
        private readonly Button recentNextButton;
        private readonly Button recentLastButton;
        private readonly Label recentPageLabel;
        private readonly Timer syncTimer;
        private bool importRunning;
        private bool accountSelectionWasExplicit;
        private ClientAccountInfo connectedAccount;
        private string summarySortColumn = "Wins";
        private bool summarySortAscending;
        private int recentPageIndex;

        public MainForm()
        {
            repository = new Repository();
            accountChoices = new List<AccountChoice>();
            accountSelectionWasExplicit =
                !String.IsNullOrWhiteSpace(
                    repository.Settings.SelectedAccountKey);
            Text = "斗魂战绩册 0.6.0";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1020, 680);
            Size = new Size(1220, 790);
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            Font = Theme.Font(9.5f, FontStyle.Regular);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(24, 18, 24, 14);
            root.BackColor = Theme.Background;
            root.RowCount = 6;
            root.ColumnCount = 1;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 174));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            Controls.Add(root);

            Panel header = new Panel();
            header.Dock = DockStyle.Fill;
            header.BackColor = Theme.Background;
            Label title = new Label();
            title.Text = "斗魂战绩册";
            title.Font = Theme.Font(22, FontStyle.Bold);
            title.ForeColor = Theme.Text;
            title.AutoSize = true;
            title.Location = new Point(0, 0);
            Label subtitle = new Label();
            subtitle.Text = "国服斗魂竞技场 · 个人英雄吃鸡统计";
            subtitle.Font = Theme.Font(9.5f, FontStyle.Regular);
            subtitle.ForeColor = Theme.Muted;
            subtitle.AutoSize = false;
            subtitle.Location = new Point(2, 34);
            subtitle.Size = new Size(420, 26);
            subtitle.TextAlign = ContentAlignment.MiddleLeft;
            subtitle.Padding = new Padding(0, 0, 0, 2);
            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            FlowLayoutPanel actions = new FlowLayoutPanel();
            actions.Dock = DockStyle.Right;
            actions.AutoSize = true;
            actions.WrapContents = false;
            actions.FlowDirection = FlowDirection.LeftToRight;
            actions.Padding = new Padding(0, 7, 0, 0);
            importButton = Theme.Button(
                "自动导入", Theme.Accent, Color.FromArgb(17, 24, 39));
            accountButton = Theme.Button(
                "选择账号  ▾", Theme.Border, Theme.Text);
            accountButton.AutoSize = false;
            accountButton.Width = 210;
            accountButton.AutoEllipsis = true;
            accountButton.TextAlign = ContentAlignment.MiddleLeft;
            accountButton.Margin = new Padding(0, 0, 6, 0);
            accountButton.Click += delegate { ShowAccountMenu(); };
            accountMenu = new ContextMenuStrip();
            accountMenu.ShowImageMargin = false;
            accountMenu.BackColor = Theme.PanelAlt;
            accountMenu.ForeColor = Theme.Text;
            accountMenu.Font = Theme.Font(9.5f, FontStyle.Regular);
            accountMenu.Padding = new Padding(4);
            accountMenu.Renderer =
                new ToolStripProfessionalRenderer(
                    new DarkMenuColorTable());
            Button privacyButton = Theme.Button(
                repository.Settings.PrivacyMode
                    ? "隐私模式：开" : "隐私模式：关",
                Theme.Border, Theme.Text);
            Button noticeButton = Theme.Button(
                "使用须知", Theme.Border, Theme.Text);
            Button directoryButton = Theme.Button("客户端目录", Theme.Border, Theme.Text);
            importButton.Click += async delegate { await StartImport(true); };
            noticeButton.Click += delegate
            {
                using (UsageNoticeDialog dialog =
                    new UsageNoticeDialog())
                    dialog.ShowDialog(this);
            };
            privacyButton.Click += delegate
            {
                repository.Settings.PrivacyMode =
                    !repository.Settings.PrivacyMode;
                repository.SaveSettings();
                privacyButton.Text = repository.Settings.PrivacyMode
                    ? "隐私模式：开" : "隐私模式：关";
                privacyButton.ForeColor = repository.Settings.PrivacyMode
                    ? Theme.Accent : Theme.Text;
                RefreshAccountBox();
                statusLabel.Text = repository.Settings.PrivacyMode
                    ? "隐私模式已开启：详情页玩家名称将匿名显示。"
                    : "隐私模式已关闭：详情页将显示玩家名称。";
            };
            privacyButton.ForeColor = repository.Settings.PrivacyMode
                ? Theme.Accent : Theme.Text;
            directoryButton.Click += delegate { ChooseClientDirectory(); };
            actions.Controls.Add(accountButton);
            actions.Controls.Add(noticeButton);
            actions.Controls.Add(importButton);
            actions.Controls.Add(privacyButton);
            actions.Controls.Add(directoryButton);
            header.Controls.Add(actions);
            root.Controls.Add(header, 0, 0);

            TableLayoutPanel cards = new TableLayoutPanel();
            cards.Dock = DockStyle.Fill;
            cards.ColumnCount = 4;
            cards.RowCount = 1;
            cards.Padding = new Padding(0, 8, 0, 8);
            for (int i = 0; i < 4; i++)
                cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            matchesValue = AddCard(cards, 0, "总对局");
            winsValue = AddCard(cards, 1, "吃鸡次数");
            rateValue = AddCard(cards, 2, "总吃鸡率");
            favoriteValue = AddCard(cards, 3, "最常用英雄");
            root.Controls.Add(cards, 0, 1);

            Panel filterPanel = new Panel();
            filterPanel.Dock = DockStyle.Fill;
            filterPanel.BackColor = Theme.Panel;
            filterPanel.Padding = new Padding(14, 9, 14, 9);
            Label statsTitle = new Label();
            statsTitle.Text = "英雄统计";
            statsTitle.Font = Theme.Font(11.5f, FontStyle.Bold);
            statsTitle.ForeColor = Theme.Text;
            statsTitle.AutoSize = true;
            statsTitle.Location = new Point(14, 12);
            filterPanel.Controls.Add(statsTitle);
            searchBox = new TextBox();
            searchBox.Width = 210;
            searchBox.BorderStyle = BorderStyle.FixedSingle;
            searchBox.BackColor = Theme.PanelAlt;
            searchBox.ForeColor = Theme.Text;
            searchBox.Location = new Point(filterPanel.Width - 224, 10);
            searchBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            searchBox.TextChanged += delegate { RefreshSummary(); };
            filterPanel.Controls.Add(searchBox);
            root.Controls.Add(filterPanel, 0, 2);

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.BackColor = Theme.Background;
            split.SplitterWidth = 10;
            split.SplitterDistance = 535;
            split.Panel1.Padding = new Padding(0, 10, 5, 0);
            split.Panel2.Padding = new Padding(5, 10, 0, 0);
            root.Controls.Add(split, 0, 3);

            summaryGrid = CreateGrid();
            summaryGrid.Columns.Add(IconColumn("Icon"));
            summaryGrid.Columns.Add(Column("Champion", "英雄", 118));
            summaryGrid.Columns.Add(Column("Picks", "选取", 50));
            summaryGrid.Columns.Add(Column("Wins", "吃鸡", 50));
            summaryGrid.Columns.Add(Column("Rate", "吃鸡率", 66));
            summaryGrid.Columns.Add(Column("Average", "平均名次", 68));
            MergeIconAndChampionHeader(summaryGrid);
            foreach (DataGridViewColumn column in summaryGrid.Columns)
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
            summaryGrid.ColumnHeaderMouseClick += SummaryHeaderClick;
            Panel summaryPanel = PanelWithTitle("按英雄汇总", summaryGrid, null);
            split.Panel1.Controls.Add(summaryPanel);

            recentGrid = CreateGrid();
            recentGrid.Columns.Add(FlexibleColumn("Time", "时间", 122));
            recentGrid.Columns["Time"].MinimumWidth = 150;
            recentGrid.Columns.Add(IconColumn("Icon"));
            recentGrid.Columns.Add(FlexibleColumn("Champion", "英雄", 82));
            recentGrid.Columns.Add(FlexibleColumn("Placement", "名次", 52));
            recentGrid.Columns.Add(FlexibleColumn("Duration", "时长", 52));
            recentGrid.Columns.Add(FlexibleColumn("Kda", "KDA", 76));
            MergeIconAndChampionHeader(recentGrid);
            recentGrid.DefaultCellStyle.Padding = new Padding(3);
            recentGrid.CellDoubleClick += delegate { ShowDetails(); };
            FlowLayoutPanel recentActions = new FlowLayoutPanel();
            recentActions.AutoSize = true;
            recentActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            recentActions.FlowDirection = FlowDirection.LeftToRight;
            recentActions.Margin = new Padding(0);
            recentActions.Padding = new Padding(0);
            Button detailsButton = Theme.Button("详情", Theme.Border, Theme.Text);
            detailsButton.Height = 32;
            detailsButton.Click += delegate { ShowDetails(); };
            recentFirstButton = CompactPageButton("«", "首页");
            recentPreviousButton = CompactPageButton("‹", "上一页");
            recentNextButton = CompactPageButton("›", "下一页");
            recentLastButton = CompactPageButton("»", "末页");
            recentPageLabel = new Label();
            recentPageLabel.AutoSize = false;
            recentPageLabel.Size = new Size(92, 32);
            recentPageLabel.ForeColor = Theme.Muted;
            recentPageLabel.TextAlign = ContentAlignment.MiddleCenter;
            recentPageLabel.Margin = new Padding(2, 0, 2, 0);
            recentFirstButton.Click += delegate { GoToRecentPage(0); };
            recentPreviousButton.Click += delegate
            {
                GoToRecentPage(recentPageIndex - 1);
            };
            recentNextButton.Click += delegate
            {
                GoToRecentPage(recentPageIndex + 1);
            };
            recentLastButton.Click += delegate
            {
                GoToRecentPage(RecentPageCount() - 1);
            };
            recentActions.Controls.Add(recentFirstButton);
            recentActions.Controls.Add(recentPreviousButton);
            recentActions.Controls.Add(recentPageLabel);
            recentActions.Controls.Add(recentNextButton);
            recentActions.Controls.Add(recentLastButton);
            recentActions.Controls.Add(detailsButton);
            Panel recentPanel = PanelWithTitle("最近对局", recentGrid, recentActions);
            split.Panel2.Controls.Add(recentPanel);

            TableLayoutPanel trendArea = new TableLayoutPanel();
            trendArea.Dock = DockStyle.Fill;
            trendArea.ColumnCount = 2;
            trendArea.RowCount = 1;
            trendArea.Padding = new Padding(0, 10, 0, 8);
            trendArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
            trendArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));

            trendChart = new PlacementTrendControl();
            trendChart.Dock = DockStyle.Fill;
            Panel trendPanel = PanelWithTitle(
                "最近 20 场名次走势", trendChart, null);
            trendPanel.Margin = new Padding(0, 0, 5, 0);
            trendArea.Controls.Add(trendPanel, 0, 0);

            TableLayoutPanel compactStats = new TableLayoutPanel();
            compactStats.Dock = DockStyle.Fill;
            compactStats.ColumnCount = 1;
            compactStats.RowCount = 3;
            compactStats.Margin = new Padding(5, 0, 0, 0);
            for (int index = 0; index < 3; index++)
                compactStats.RowStyles.Add(
                    new RowStyle(SizeType.Percent, 33.333f));
            streakValue = AddMiniMetric(
                compactStats, 0, "当前连续未吃鸡", "场");
            todayValue = AddMiniMetric(compactStats, 1, "今日对局", "场");
            weekValue = AddMiniMetric(compactStats, 2, "本周对局", "场");
            trendArea.Controls.Add(compactStats, 1, 0);
            root.Controls.Add(trendArea, 0, 4);

            TableLayoutPanel statusPanel = new TableLayoutPanel();
            statusPanel.Dock = DockStyle.Fill;
            statusPanel.BackColor = Theme.PanelAlt;
            statusPanel.ColumnCount = 3;
            statusPanel.RowCount = 1;
            statusPanel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 30));
            statusPanel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));
            statusPanel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 200));
            connectionDot = new StatusDot();
            connectionDot.Dock = DockStyle.Fill;
            connectionDot.Margin = new Padding(5, 0, 0, 0);
            statusLabel = new Label();
            statusLabel.Text = "就绪";
            statusLabel.ForeColor = Theme.Muted;
            statusLabel.AutoEllipsis = true;
            statusLabel.Dock = DockStyle.Fill;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            statusLabel.Padding = new Padding(6, 0, 0, 0);
            Label version = new Label();
            version.Text = "数据仅保存在本机 · v0.6.0";
            version.ForeColor = Theme.Muted;
            version.Dock = DockStyle.Right;
            version.Width = 200;
            version.TextAlign = ContentAlignment.MiddleRight;
            version.Padding = new Padding(0, 0, 12, 0);
            statusPanel.Controls.Add(connectionDot, 0, 0);
            statusPanel.Controls.Add(statusLabel, 1, 0);
            statusPanel.Controls.Add(version, 2, 0);
            root.Controls.Add(statusPanel, 0, 5);

            syncTimer = new Timer();
            syncTimer.Interval = 60000;
            syncTimer.Tick += async delegate { await StartImport(false); };
            if (repository.Settings.AutoSync)
                syncTimer.Start();

            RefreshAccountBox();

            Shown += async delegate
            {
                split.SplitterDistance = Math.Max(
                    410, Math.Min(split.Width - 410, (int)(split.Width * 0.50))
                );
                RefreshAll();
                if (repository.Settings.AutoSync)
                    await StartImport(false);
            };
        }

        private static Label AddCard(TableLayoutPanel cards, int column, string title)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Theme.Panel;
            panel.Margin = new Padding(column == 0 ? 0 : 6, 0, column == 3 ? 0 : 6, 0);
            Label name = new Label();
            name.Text = title;
            name.ForeColor = Theme.Muted;
            name.Font = Theme.Font(9, FontStyle.Regular);
            name.AutoSize = true;
            name.Location = new Point(16, 13);
            Label value = new Label();
            value.Text = "0";
            value.ForeColor = Theme.Text;
            value.Font = Theme.Font(18, FontStyle.Bold);
            value.AutoEllipsis = true;
            value.AutoSize = false;
            value.Location = new Point(15, 37);
            value.Size = new Size(230, 36);
            value.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel.Controls.Add(name);
            panel.Controls.Add(value);
            cards.Controls.Add(panel, column, 0);
            return value;
        }

        private static Label AddMiniMetric(
            TableLayoutPanel container, int row, string title, string suffix)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Theme.Panel;
            panel.Margin = new Padding(
                0, row == 0 ? 0 : 3, 0, row == 2 ? 0 : 3);

            Label name = new Label();
            name.Text = title;
            name.ForeColor = Theme.Muted;
            name.Font = Theme.Font(8.5f, FontStyle.Regular);
            name.AutoSize = true;
            name.Location = new Point(14, 9);

            Label value = new Label();
            value.Text = "0 " + suffix;
            value.ForeColor = Theme.Text;
            value.Font = Theme.Font(13, FontStyle.Bold);
            value.AutoSize = false;
            value.TextAlign = ContentAlignment.MiddleRight;
            value.Dock = DockStyle.Right;
            value.Width = 96;
            value.Padding = new Padding(0, 0, 14, 0);

            panel.Controls.Add(name);
            panel.Controls.Add(value);
            container.Controls.Add(panel, 0, row);
            return value;
        }

        private static Button CompactPageButton(
            string text, string accessibleName)
        {
            Button button = Theme.Button(
                text, Theme.Border, Theme.Text);
            button.AutoSize = false;
            button.Size = new Size(38, 32);
            button.Padding = new Padding(4, 0, 4, 0);
            button.Margin = new Padding(2, 0, 2, 0);
            button.Font = Theme.Font(8.5f, FontStyle.Bold);
            button.AccessibleName = accessibleName;
            return button;
        }

        private static Panel PanelWithTitle(
            string titleText, Control content, Control actions)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Theme.Panel;
            panel.Padding = new Padding(12);
            Label title = new Label();
            title.Text = titleText;
            title.ForeColor = Theme.Text;
            title.Font = Theme.Font(11, FontStyle.Bold);
            title.AutoSize = true;
            title.Location = new Point(12, 13);
            panel.Controls.Add(title);
            if (actions != null)
            {
                actions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                panel.Controls.Add(actions);
                Action positionActions = delegate
                {
                    actions.Location = new Point(
                        Math.Max(
                            title.Right + 12,
                            panel.ClientSize.Width - actions.Width - 12),
                        7
                    );
                };
                panel.SizeChanged += delegate { positionActions(); };
                actions.SizeChanged += delegate { positionActions(); };
                positionActions();
            }
            content.Location = new Point(12, 50);
            content.Size = new Size(panel.Width - 24, panel.Height - 62);
            content.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                AnchorStyles.Left | AnchorStyles.Right;
            panel.Controls.Add(content);
            return panel;
        }

        private static DataGridView CreateGrid()
        {
            DataGridView grid = new DataGridView();
            grid.BackgroundColor = Theme.PanelAlt;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = Theme.Border;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersHeight = 38;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Panel;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Muted;
            grid.ColumnHeadersDefaultCellStyle.Font = Theme.Font(9, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Panel;
            grid.DefaultCellStyle.BackColor = Theme.PanelAlt;
            grid.DefaultCellStyle.ForeColor = Theme.Text;
            grid.DefaultCellStyle.SelectionBackColor = Theme.Selection;
            grid.DefaultCellStyle.SelectionForeColor = Theme.Text;
            grid.DefaultCellStyle.Padding = new Padding(5);
            grid.RowTemplate.Height = 42;
            grid.CellPainting += PaintColumnHeader;
            return grid;
        }

        private static void PaintColumnHeader(
            object sender, DataGridViewCellPaintingEventArgs eventArgs)
        {
            if (eventArgs.RowIndex != -1 || eventArgs.ColumnIndex < 0)
                return;
            DataGridView grid = sender as DataGridView;
            if (grid == null)
                return;

            Rectangle area = eventArgs.CellBounds;
            using (Brush background = new SolidBrush(Theme.Panel))
                eventArgs.Graphics.FillRectangle(background, area);
            using (Pen border = new Pen(Theme.Border))
            {
                eventArgs.Graphics.DrawRectangle(
                    border,
                    area.X,
                    area.Y,
                    Math.Max(0, area.Width - 1),
                    Math.Max(0, area.Height - 1));
            }

            DataGridViewColumn column =
                grid.Columns[eventArgs.ColumnIndex];
            Rectangle textArea = new Rectangle(
                area.X + 9,
                area.Y,
                Math.Max(0, area.Width - 18),
                area.Height);
            TextRenderer.DrawText(
                eventArgs.Graphics,
                column.HeaderText ?? "",
                grid.ColumnHeadersDefaultCellStyle.Font,
                textArea,
                Theme.Muted,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine |
                TextFormatFlags.EndEllipsis);

            SortOrder direction =
                column.HeaderCell.SortGlyphDirection;
            if (direction != SortOrder.None)
            {
                int centerX = area.Right - 13;
                int centerY = area.Top + area.Height / 2;
                Point[] triangle = direction == SortOrder.Ascending
                    ? new[]
                    {
                        new Point(centerX, centerY - 3),
                        new Point(centerX - 4, centerY + 3),
                        new Point(centerX + 4, centerY + 3)
                    }
                    : new[]
                    {
                        new Point(centerX - 4, centerY - 3),
                        new Point(centerX + 4, centerY - 3),
                        new Point(centerX, centerY + 3)
                    };
                using (Brush glyph = new SolidBrush(Theme.Muted))
                    eventArgs.Graphics.FillPolygon(glyph, triangle);
            }
            eventArgs.Handled = true;
        }

        private static DataGridViewTextBoxColumn Column(
            string name, string title, int minimumWidth)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.Name = name;
            column.HeaderText = title;
            column.MinimumWidth = minimumWidth;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            return column;
        }

        private static DataGridViewTextBoxColumn FlexibleColumn(
            string name, string title, float fillWeight)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.Name = name;
            column.HeaderText = title;
            column.MinimumWidth = 34;
            column.FillWeight = fillWeight;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            return column;
        }

        private static DataGridViewImageColumn IconColumn(string name)
        {
            DataGridViewImageColumn column =
                new DataGridViewImageColumn();
            column.Name = name;
            column.HeaderText = "";
            column.MinimumWidth = 42;
            column.Width = 42;
            column.FillWeight = 36;
            column.ImageLayout = DataGridViewImageCellLayout.Zoom;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            column.DefaultCellStyle.NullValue = null;
            column.DefaultCellStyle.Padding = new Padding(4);
            return column;
        }

        private static void MergeIconAndChampionHeader(DataGridView grid)
        {
            grid.Paint += delegate(object sender, PaintEventArgs eventArgs)
            {
                if (!grid.Columns.Contains("Icon") ||
                    !grid.Columns.Contains("Champion"))
                    return;
                DataGridViewColumn icon = grid.Columns["Icon"];
                DataGridViewColumn champion = grid.Columns["Champion"];
                Rectangle iconArea = grid.GetCellDisplayRectangle(
                    icon.Index, -1, true);
                Rectangle championArea = grid.GetCellDisplayRectangle(
                    champion.Index, -1, true);
                if (iconArea.Width <= 0 || championArea.Width <= 0)
                    return;
                Rectangle area = Rectangle.Union(iconArea, championArea);
                using (Brush background = new SolidBrush(Theme.Panel))
                    eventArgs.Graphics.FillRectangle(background, area);
                using (Pen border = new Pen(Theme.Border))
                {
                    eventArgs.Graphics.DrawRectangle(
                        border, area.X, area.Y,
                        Math.Max(0, area.Width - 1),
                        Math.Max(0, area.Height - 1));
                }
                TextRenderer.DrawText(
                    eventArgs.Graphics,
                    "英雄",
                    grid.ColumnHeadersDefaultCellStyle.Font,
                    area,
                    Theme.Muted,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine);

                SortOrder direction =
                    champion.HeaderCell.SortGlyphDirection;
                if (direction != SortOrder.None)
                {
                    int centerX = area.Right - 13;
                    int centerY = area.Top + area.Height / 2;
                    Point[] triangle = direction == SortOrder.Ascending
                        ? new[]
                        {
                            new Point(centerX, centerY - 3),
                            new Point(centerX - 4, centerY + 3),
                            new Point(centerX + 4, centerY + 3)
                        }
                        : new[]
                        {
                            new Point(centerX - 4, centerY - 3),
                            new Point(centerX + 4, centerY - 3),
                            new Point(centerX, centerY + 3)
                        };
                    using (Brush glyph = new SolidBrush(Theme.Muted))
                        eventArgs.Graphics.FillPolygon(glyph, triangle);
                }
            };
        }

        private void RefreshAccountBox()
        {
            if (accountButton == null)
                return;
            string selectedKey = repository.ActiveAccountKey;
            accountChoices.Clear();
            List<AccountProfile> profiles =
                repository.AccountProfiles();
            foreach (AccountProfile profile in profiles)
            {
                bool unassigned = String.Equals(
                    profile.Key,
                    Repository.UnassignedAccountKey,
                    StringComparison.OrdinalIgnoreCase);
                string name;
                if (unassigned)
                {
                    name = "未归属数据";
                }
                else if (repository.Settings.PrivacyMode)
                {
                    int stableNumber =
                        repository.Settings.Accounts.FindIndex(
                            item => String.Equals(
                                item.Key, profile.Key,
                                StringComparison.OrdinalIgnoreCase)) + 1;
                    name = "账号 " + Math.Max(1, stableNumber).ToString(
                        CultureInfo.InvariantCulture);
                }
                else
                {
                    name = String.IsNullOrWhiteSpace(profile.DisplayName)
                        ? "历史账号" : profile.DisplayName;
                }
                bool connected = connectedAccount != null &&
                    String.Equals(
                        connectedAccount.Key, profile.Key,
                        StringComparison.OrdinalIgnoreCase);
                string label = (connected ? "● " : "") + name +
                    "  (" + repository.MatchCountForAccount(profile.Key)
                        .ToString(CultureInfo.InvariantCulture) + " 场)";
                accountChoices.Add(new AccountChoice
                {
                    Key = profile.Key,
                    Label = label
                });
            }
            AccountChoice selected = accountChoices.FirstOrDefault(choice =>
                String.Equals(
                    choice.Key, selectedKey,
                    StringComparison.OrdinalIgnoreCase));
            if (selected == null && accountChoices.Count > 0)
            {
                selected = accountChoices[0];
                repository.SetActiveAccount(selected.Key);
            }
            accountButton.Text =
                (selected == null ? "选择账号" : selected.Label) + "  ▾";
            UpdateImportAvailability();
        }

        private void ShowAccountMenu()
        {
            if (accountChoices.Count == 0 || importRunning)
                return;
            if (accountMenu.Visible)
            {
                accountMenu.Close(
                    ToolStripDropDownCloseReason.AppClicked);
                return;
            }
            foreach (ToolStripItem oldItem in
                accountMenu.Items.Cast<ToolStripItem>().ToArray())
                oldItem.Dispose();
            accountMenu.Items.Clear();
            foreach (AccountChoice item in accountChoices)
            {
                AccountChoice choice = item;
                ToolStripMenuItem menuItem =
                    new ToolStripMenuItem(choice.Label);
                menuItem.BackColor = Theme.PanelAlt;
                menuItem.ForeColor = String.Equals(
                    choice.Key, repository.ActiveAccountKey,
                    StringComparison.OrdinalIgnoreCase)
                    ? Theme.Accent : Theme.Text;
                menuItem.Padding = new Padding(8, 5, 18, 5);
                menuItem.Click += delegate { SelectAccount(choice.Key); };
                accountMenu.Items.Add(menuItem);
            }
            accountMenu.Show(
                accountButton,
                new Point(0, accountButton.Height + 2));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && accountMenu != null)
                accountMenu.Dispose();
            base.Dispose(disposing);
        }

        private void SelectAccount(string accountKey)
        {
            AccountChoice choice = accountChoices.FirstOrDefault(item =>
                String.Equals(
                    item.Key, accountKey,
                    StringComparison.OrdinalIgnoreCase));
            if (choice == null)
                return;
            accountSelectionWasExplicit = true;
            repository.SetActiveAccount(choice.Key);
            recentPageIndex = 0;
            RefreshAccountBox();
            RefreshAll();
            UpdateImportAvailability();
            if (connectedAccount != null && String.Equals(
                choice.Key, connectedAccount.Key,
                StringComparison.OrdinalIgnoreCase))
            {
                statusLabel.Text =
                    "正在查看当前客户端账号，可以导入数据。";
            }
            else
            {
                statusLabel.Text =
                    "正在查看历史账号：只读模式，自动导入已暂停。";
            }
        }

        private void UpdateImportAvailability()
        {
            if (importButton == null)
                return;
            bool sameAccount = connectedAccount != null &&
                String.Equals(
                    repository.ActiveAccountKey,
                    connectedAccount.Key,
                    StringComparison.OrdinalIgnoreCase);
            bool canProbeOrImport =
                connectedAccount == null || sameAccount;
            importButton.Enabled = !importRunning && canProbeOrImport;
            importButton.Text = importRunning ? "导入中…" : "自动导入";
            if (accountButton != null)
                accountButton.Enabled = !importRunning;
        }

        private void RefreshAll()
        {
            int count = repository.MatchCount;
            int wins = repository.WinCount;
            List<HeroSummary> summary = repository.Summary();
            List<MatchRecord> allMatches = repository.Recent(Int32.MaxValue);
            matchesValue.Text = count.ToString(CultureInfo.InvariantCulture);
            winsValue.Text = wins.ToString(CultureInfo.InvariantCulture);
            rateValue.Text = count == 0 ? "0.0%" :
                (100.0 * wins / count).ToString("0.0", CultureInfo.InvariantCulture) + "%";
            favoriteValue.Text = summary.Count == 0 ? "暂无" : summary[0].ChampionName;

            int streak = 0;
            foreach (MatchRecord match in allMatches)
            {
                if (match.Placement == 1)
                    break;
                streak++;
            }
            DateTime today = DateTime.Today;
            DateTime weekStart = today.AddDays(
                -(((int)today.DayOfWeek + 6) % 7));
            streakValue.Text = streak.ToString(CultureInfo.InvariantCulture) + " 场";
            todayValue.Text = allMatches.Count(
                x => x.PlayedAt.Date == today).ToString(
                    CultureInfo.InvariantCulture) + " 场";
            weekValue.Text = allMatches.Count(
                x => x.PlayedAt >= weekStart &&
                    x.PlayedAt < weekStart.AddDays(7)).ToString(
                    CultureInfo.InvariantCulture) + " 场";
            trendChart.SetData(allMatches.Take(20).Reverse());
            RefreshSummary();
            RefreshRecent();
        }

        private void RefreshSummary()
        {
            if (summaryGrid == null)
                return;
            string search = searchBox == null ? "" : searchBox.Text.Trim();
            IEnumerable<HeroSummary> items = repository.Summary();
            Func<HeroSummary, object> selector;
            switch (summarySortColumn)
            {
                case "Champion":
                    selector = x => x.ChampionName;
                    break;
                case "Picks":
                    selector = x => x.Picks;
                    break;
                case "Rate":
                    selector = x => x.WinRate;
                    break;
                case "Average":
                    selector = x => x.AveragePlacement;
                    break;
                default:
                    selector = x => x.Wins;
                    break;
            }
            items = summarySortAscending
                ? items.OrderBy(selector).ThenBy(x => x.ChampionName)
                : items.OrderByDescending(selector).ThenBy(x => x.ChampionName);

            summaryGrid.Rows.Clear();
            foreach (HeroSummary item in items)
            {
                if (!String.IsNullOrWhiteSpace(search) &&
                    item.ChampionName.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                summaryGrid.Rows.Add(
                    ChampionIconCache.Get(item.ChampionId),
                    item.ChampionName,
                    item.Picks,
                    item.Wins,
                    item.WinRate.ToString("0.0", CultureInfo.InvariantCulture) + "%",
                    item.AveragePlacement.ToString("0.00", CultureInfo.InvariantCulture)
                );
            }
            foreach (DataGridViewColumn column in summaryGrid.Columns)
                column.HeaderCell.SortGlyphDirection = SortOrder.None;
            if (summaryGrid.Columns.Contains(summarySortColumn))
                summaryGrid.Columns[summarySortColumn].HeaderCell.SortGlyphDirection =
                    summarySortAscending ? SortOrder.Ascending : SortOrder.Descending;
            summaryGrid.ClearSelection();
        }

        private void SummaryHeaderClick(
            object sender, DataGridViewCellMouseEventArgs eventArgs)
        {
            if (eventArgs.ColumnIndex < 0)
                return;
            string column = summaryGrid.Columns[eventArgs.ColumnIndex].Name;
            if (String.Equals(column, "Icon", StringComparison.Ordinal))
                column = "Champion";
            if (String.Equals(
                column, summarySortColumn, StringComparison.Ordinal))
                summarySortAscending = !summarySortAscending;
            else
            {
                summarySortColumn = column;
                summarySortAscending = column == "Champion" ||
                    column == "Average";
            }
            RefreshSummary();
        }

        private int RecentPageCount()
        {
            return Math.Max(
                1,
                (repository.MatchCount + RecentPageSize - 1) /
                    RecentPageSize);
        }

        private void GoToRecentPage(int pageIndex)
        {
            int lastPage = RecentPageCount() - 1;
            recentPageIndex = Math.Max(
                0, Math.Min(pageIndex, lastPage));
            RefreshRecent();
        }

        private void RefreshRecent()
        {
            int total = repository.MatchCount;
            int pageCount = Math.Max(
                1,
                (total + RecentPageSize - 1) / RecentPageSize);
            recentPageIndex = Math.Max(
                0, Math.Min(recentPageIndex, pageCount - 1));
            recentGrid.Rows.Clear();
            foreach (MatchRecord item in repository.RecentPage(
                recentPageIndex * RecentPageSize,
                RecentPageSize))
            {
                string kda = item.Kills.HasValue && item.Deaths.HasValue &&
                    item.Assists.HasValue
                    ? String.Format(
                        CultureInfo.InvariantCulture,
                        "{0}/{1}/{2}",
                        item.Kills.Value, item.Deaths.Value, item.Assists.Value)
                    : "—";
                int rowIndex = recentGrid.Rows.Add(
                    item.PlayedAt.ToString("yyyy-MM-dd HH:mm"),
                    ChampionIconCache.Get(item.ChampionId),
                    item.ChampionName,
                    "第 " + item.Placement.ToString(CultureInfo.InvariantCulture) + " 名",
                    FormatDurationShort(item.DurationSeconds),
                    kda
                );
                recentGrid.Rows[rowIndex].Tag = item;
                if (item.Placement == 1)
                    recentGrid.Rows[rowIndex].DefaultCellStyle.ForeColor =
                        Color.FromArgb(247, 202, 105);
            }
            recentGrid.ClearSelection();
            recentPageLabel.Text = String.Format(
                CultureInfo.InvariantCulture,
                "{0} / {1} · {2} 场",
                recentPageIndex + 1,
                pageCount,
                total);
            recentFirstButton.Enabled = recentPageIndex > 0;
            recentPreviousButton.Enabled = recentPageIndex > 0;
            recentNextButton.Enabled =
                recentPageIndex < pageCount - 1;
            recentLastButton.Enabled =
                recentPageIndex < pageCount - 1;
        }

        private static string FormatDurationShort(int? seconds)
        {
            if (!seconds.HasValue || seconds.Value <= 0)
                return "—";
            return String.Format(
                CultureInfo.InvariantCulture,
                "{0}:{1:00}",
                seconds.Value / 60,
                seconds.Value % 60
            );
        }

        private async Task StartImport(bool interactive)
        {
            if (importRunning)
                return;
            importRunning = true;
            UpdateImportAvailability();
            statusLabel.Text = "正在连接国服客户端并读取最近对局…";
            connectionDot.StateColor = Theme.Accent;
            try
            {
                string preferred = repository.Settings.ClientRoot;
                ClientAccountInfo probedAccount = await Task.Run(
                    delegate
                    {
                        return new LcuClient(preferred).ProbeAccount();
                    });
                if (String.IsNullOrWhiteSpace(probedAccount.Key))
                    throw new LcuException(
                        "无法识别当前客户端账号，请重新登录后重试。");
                connectedAccount = probedAccount;
                repository.UpsertAccount(
                    probedAccount.Key,
                    probedAccount.DisplayName,
                    probedAccount.ProfileIconId);
                if (!accountSelectionWasExplicit)
                {
                    repository.SetActiveAccount(probedAccount.Key);
                    accountSelectionWasExplicit = true;
                    RefreshAll();
                }
                RefreshAccountBox();
                if (!String.Equals(
                    repository.ActiveAccountKey,
                    probedAccount.Key,
                    StringComparison.OrdinalIgnoreCase))
                {
                    connectionDot.StateColor = Theme.Success;
                    statusLabel.Text =
                        "客户端连接正常，但当前正在查看其他账号；" +
                        "该页面为只读模式，导入已暂停。";
                    if (interactive)
                        MessageBox.Show(
                            this,
                            "当前查看账号与客户端登录账号不同。\n" +
                            "请在顶部切换到带圆点的账号后再导入。",
                            "当前账号仅可查看",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    return;
                }
                List<string> knownParticipantDetails =
                    repository.ParticipantDetailsLoadedGameIds();
                List<int> knownChampionIds = repository.ChampionIds();
                List<string> claimGameIds =
                    repository.UnassignedGameIdsForAccount(
                        probedAccount.Key, 12);
                ImportResult result = await Task.Run(
                    delegate
                    {
                        return new LcuClient(preferred).Fetch(
                            100, knownParticipantDetails,
                            knownChampionIds, claimGameIds);
                    }
                );
                if (String.IsNullOrWhiteSpace(result.AccountKey) ||
                    !String.Equals(
                        result.AccountKey,
                        probedAccount.Key,
                        StringComparison.OrdinalIgnoreCase) ||
                    !String.Equals(
                        repository.ActiveAccountKey,
                        result.AccountKey,
                        StringComparison.OrdinalIgnoreCase))
                    throw new LcuException(
                        "检测到客户端账号发生变化，本次导入已取消。");
                repository.UpsertAccount(
                    result.AccountKey,
                    result.AccountDisplayName,
                    result.AccountProfileIconId);
                int claimed = repository.ApplyClaimResults(
                    result.ClaimedGameIds,
                    result.CheckedClaimGameIds,
                    result.AccountKey);
                int inserted = repository.Import(
                    result.Matches, result.AccountKey);
                repository.MarkAccountImported(result.AccountKey);
                int participantDetailsLoaded = result.Matches.Count(
                    x => x.ParticipantDetailsLoaded);
                repository.Settings.ClientRoot = result.ClientRoot;
                repository.SaveSettings();
                RefreshAll();
                RefreshAccountBox();
                connectionDot.StateColor = Theme.Success;
                statusLabel.Text = String.Format(
                    "客户端连接正常：识别 {0} 场斗魂对局，新增 {1} 场，" +
                    "认领旧数据 {2} 场，补全 {3} 场（阵容 {4} 场）。",
                    result.Matches.Count, inserted, claimed,
                    repository.LastUpdatedCount, participantDetailsLoaded
                );
                if (interactive)
                    MessageBox.Show(
                        this,
                        String.Format(
                            "读取历史对局：{0} 场\n识别斗魂竞技场：{1} 场\n" +
                            "本次新增：{2} 场\n认领旧数据：{3} 场\n" +
                            "补全已有记录：{4} 场\n本次补全阵容：{5} 场",
                            result.HistoryCount, result.Matches.Count, inserted,
                            claimed, repository.LastUpdatedCount,
                            participantDetailsLoaded),
                        "导入完成", MessageBoxButtons.OK, MessageBoxIcon.Information
                    );
            }
            catch (Exception exception)
            {
                connectedAccount = null;
                connectionDot.StateColor = Theme.Danger;
                statusLabel.Text = exception.Message;
                if (interactive)
                    MessageBox.Show(
                        this, exception.Message, "无法自动导入",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
            }
            finally
            {
                importRunning = false;
                UpdateImportAvailability();
            }
        }

        private void AddManual()
        {
            using (MatchEditDialog dialog = new MatchEditDialog(
                repository.ChampionNames(), null))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                repository.AddManual(
                    dialog.ChampionNameValue,
                    dialog.PlacementValue,
                    dialog.PlayedAtValue
                );
            }
            RefreshAll();
            statusLabel.Text = "已添加一场手动对局。";
        }

        private MatchRecord SelectedRecord()
        {
            if (recentGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    this, "请先选择一条对局记录。", "未选择对局",
                    MessageBoxButtons.OK, MessageBoxIcon.Information
                );
                return null;
            }
            return recentGrid.SelectedRows[0].Tag as MatchRecord;
        }

        private void ShowDetails()
        {
            MatchRecord record = SelectedRecord();
            if (record == null)
                return;
            using (MatchDetailsDialog dialog = new MatchDetailsDialog(
                record, repository.Settings.PrivacyMode))
                dialog.ShowDialog(this);
        }

        private void EditSelected()
        {
            MatchRecord record = SelectedRecord();
            if (record == null)
                return;
            using (MatchEditDialog dialog = new MatchEditDialog(
                repository.ChampionNames(), record))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                repository.Update(
                    record.GameId,
                    dialog.ChampionNameValue,
                    dialog.PlacementValue,
                    dialog.PlayedAtValue
                );
            }
            RefreshAll();
            statusLabel.Text = "对局记录已更新。";
        }

        private void DeleteSelected()
        {
            MatchRecord record = SelectedRecord();
            if (record == null)
                return;
            if (MessageBox.Show(
                this,
                "确定删除“" + record.ChampionName + " 第 " +
                    record.Placement.ToString(CultureInfo.InvariantCulture) +
                    " 名”这条记录吗？",
                "删除对局", MessageBoxButtons.YesNo, MessageBoxIcon.Warning
            ) != DialogResult.Yes)
                return;
            repository.Delete(record.GameId);
            RefreshAll();
            statusLabel.Text = "对局记录已删除。";
        }

        private void ChooseClientDirectory()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择包含 LeagueClient.exe 的 LeagueClient 文件夹";
                dialog.SelectedPath = repository.Settings.ClientRoot;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                if (!File.Exists(Path.Combine(dialog.SelectedPath, "LeagueClient.exe")))
                {
                    MessageBox.Show(
                        this,
                        "所选目录中没有 LeagueClient.exe。",
                        "目录不正确",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }
                repository.Settings.ClientRoot = dialog.SelectedPath;
                repository.SaveSettings();
                statusLabel.Text = "客户端目录已设置：" + dialog.SelectedPath;
            }
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length > 1 &&
                String.Equals(
                    args[0], "--preview-details",
                    StringComparison.OrdinalIgnoreCase))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Repository previewRepository = new Repository(args[1]);
                MatchRecord preview = previewRepository.Recent(1).FirstOrDefault();
                if (preview != null)
                    Application.Run(new MatchDetailsDialog(preview));
                return;
            }
            if (args.Length > 0 &&
                String.Equals(args[0], "--self-test", StringComparison.OrdinalIgnoreCase))
            {
                RunSelfTest(args);
                return;
            }
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += delegate(
                    object sender, System.Threading.ThreadExceptionEventArgs eventArgs)
                {
                    LogCrash(eventArgs.Exception);
                    MessageBox.Show(
                        eventArgs.Exception.ToString(),
                        "斗魂战绩册发生错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                };
                Application.Run(new MainForm());
            }
            catch (Exception exception)
            {
                LogCrash(exception);
                MessageBox.Show(
                    exception.ToString(),
                    "斗魂战绩册无法启动",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private static void LogCrash(Exception exception)
        {
            try
            {
                Directory.CreateDirectory(AppPaths.AppDirectory);
                File.WriteAllText(
                    Path.Combine(AppPaths.AppDirectory, "crash.log"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                    Environment.NewLine + exception,
                    new UTF8Encoding(false)
                );
            }
            catch { }
        }

        private static void RunSelfTest(string[] args)
        {
            string outputDirectory = args.Length > 1
                ? args[1]
                : Path.Combine(Path.GetTempPath(), "ArenaTrackerSelfTest");
            Directory.CreateDirectory(outputDirectory);
            Dictionary<string, object> report = new Dictionary<string, object>();
            try
            {
                Repository repository = new Repository(outputDirectory);
                LcuClient client = new LcuClient("");
                ClientAccountInfo account = client.ProbeAccount();
                List<string> legacyClaims =
                    repository.UnassignedGameIdsForAccount(account.Key, 12);
                ImportResult result = client.Fetch(
                    100,
                    repository.ParticipantDetailsLoadedGameIds(),
                    repository.ChampionIds(),
                    legacyClaims);
                repository.UpsertAccount(
                    result.AccountKey,
                    result.AccountDisplayName,
                    result.AccountProfileIconId);
                repository.SetActiveAccount(result.AccountKey);
                int legacyClaimed = repository.ApplyClaimResults(
                    result.ClaimedGameIds,
                    result.CheckedClaimGameIds,
                    result.AccountKey);
                int first = repository.Import(
                    result.Matches, result.AccountKey);
                MatchRecord latest = repository.Recent(1).FirstOrDefault();
                if (latest != null)
                {
                    latest.DurationSeconds = null;
                    latest.Items = new List<NamedGameObject>();
                    latest.Augments = new List<NamedGameObject>();
                    latest.ParticipantDetailsLoaded = false;
                    latest.Teammates = new List<PlayerMatchRecord>();
                    latest.Opponents = new List<PlayerMatchRecord>();
                }
                ImportResult refreshed = client.Fetch(
                    100, repository.ParticipantDetailsLoadedGameIds());
                int second = repository.Import(
                    refreshed.Matches, refreshed.AccountKey);
                int enriched = repository.LastUpdatedCount;
                latest = repository.Recent(1).FirstOrDefault();
                bool detailsPresent = latest != null &&
                    latest.DurationSeconds.HasValue &&
                    latest.Kills.HasValue &&
                    latest.GoldEarned.HasValue &&
                    latest.DamageToChampions.HasValue &&
                    latest.DamageTaken.HasValue &&
                    latest.TotalHeal.HasValue &&
                    latest.DamageSelfMitigated.HasValue &&
                    latest.Items != null && latest.Items.Count > 0 &&
                    latest.Augments != null && latest.Augments.Count > 0 &&
                    latest.ParticipantDetailsLoaded &&
                    latest.Teammates != null && latest.Teammates.Count > 0 &&
                    latest.Opponents != null && latest.Opponents.Count > 0 &&
                    latest.Teammates.All(player => player.SubteamId != 0) &&
                    latest.Opponents.All(player => player.SubteamId != 0);
                int importedCount = repository.MatchCount;
                repository.AddManual("测试英雄", 3, DateTime.Now.AddMinutes(1));
                MatchRecord manual = repository.Recent(1).FirstOrDefault();
                bool manualAdded = repository.MatchCount == importedCount + 1 &&
                    manual != null && manual.Source == "manual";
                if (manual != null)
                {
                    repository.Update(
                        manual.GameId, "测试英雄", 1, manual.PlayedAt);
                    manual = repository.Recent(1).FirstOrDefault();
                }
                bool manualUpdated = manual != null && manual.Placement == 1;
                if (manual != null)
                    repository.Delete(manual.GameId);
                bool manualDeleted = repository.MatchCount == importedCount;
                Repository reloaded = new Repository(outputDirectory);
                MatchRecord reloadedLatest =
                    reloaded.Recent(1).FirstOrDefault();
                bool timeRoundTripLocal = latest != null &&
                    reloadedLatest != null &&
                    reloadedLatest.PlayedAt.Kind == DateTimeKind.Local &&
                    reloadedLatest.PlayedAt == latest.PlayedAt;

                string isolationDirectory = Path.Combine(
                    outputDirectory, "account-isolation");
                Directory.CreateDirectory(isolationDirectory);
                MatchRecord legacyShared = new MatchRecord
                {
                    GameId = "shared-game",
                    PlayedAt = DateTime.Now.AddHours(-2),
                    ChampionName = "账号A英雄",
                    Placement = 1
                };
                MatchRecord legacyUnknown = new MatchRecord
                {
                    GameId = "unknown-game",
                    PlayedAt = DateTime.Now.AddHours(-3),
                    ChampionName = "未归属英雄",
                    Placement = 4
                };
                MatchRecord legacyClaimable = new MatchRecord
                {
                    GameId = "claim-game",
                    PlayedAt = DateTime.Now.AddHours(-4),
                    ChampionName = "待认领英雄",
                    Placement = 2
                };
                JavaScriptSerializer isolationSerializer =
                    new JavaScriptSerializer();
                File.WriteAllText(
                    Path.Combine(isolationDirectory, "matches.json"),
                    isolationSerializer.Serialize(
                        new[]
                        {
                            legacyShared,
                            legacyUnknown,
                            legacyClaimable
                        }),
                    new UTF8Encoding(false));
                Repository isolated =
                    new Repository(isolationDirectory);
                isolated.UpsertAccount("account-a", "账号 A", 0);
                isolated.SetActiveAccount("account-a");
                isolated.Import(
                    new[]
                    {
                        new MatchRecord
                        {
                            GameId = "shared-game",
                            PlayedAt = legacyShared.PlayedAt,
                            ChampionName = "账号A英雄",
                            Placement = 1
                        }
                    },
                    "account-a");
                bool accountAIsolated =
                    isolated.MatchCount == 1 &&
                    isolated.WinCount == 1;
                int syntheticClaimed = isolated.ApplyClaimResults(
                    new[] { "claim-game" },
                    new[] { "claim-game" },
                    "account-a");
                bool claimApplied =
                    syntheticClaimed == 1 &&
                    isolated.MatchCount == 2;
                isolated.UpsertAccount("account-b", "账号 B", 0);
                isolated.SetActiveAccount("account-b");
                isolated.Import(
                    new[]
                    {
                        new MatchRecord
                        {
                            GameId = "shared-game",
                            PlayedAt = legacyShared.PlayedAt,
                            ChampionName = "账号B英雄",
                            Placement = 6
                        },
                        new MatchRecord
                        {
                            GameId = "account-b-game",
                            PlayedAt = DateTime.Now,
                            ChampionName = "账号B英雄",
                            Placement = 2
                        }
                    },
                    "account-b");
                bool accountBIsolated =
                    isolated.MatchCount == 2 &&
                    isolated.WinCount == 0 &&
                    isolated.Summary().Count == 1;
                isolated.SetActiveAccount(
                    Repository.UnassignedAccountKey);
                bool legacyPreserved =
                    isolated.MatchCount == 1 &&
                    isolated.Recent(1)[0].GameId == "unknown-game";
                Repository isolatedReloaded =
                    new Repository(isolationDirectory);
                bool selectedAccountPersisted = String.Equals(
                    isolatedReloaded.ActiveAccountKey,
                    Repository.UnassignedAccountKey,
                    StringComparison.OrdinalIgnoreCase);
                bool accountIsolation = accountAIsolated &&
                    accountBIsolated && legacyPreserved && claimApplied &&
                    selectedAccountPersisted;

                report["success"] = detailsPresent &&
                    timeRoundTripLocal && accountIsolation;
                report["historyCount"] = result.HistoryCount;
                report["arenaMatches"] = result.Matches.Count;
                report["firstInsert"] = first;
                report["duplicateInsert"] = second;
                report["enrichedExisting"] = enriched;
                report["detailsPresent"] = detailsPresent;
                report["timeRoundTripLocal"] = timeRoundTripLocal;
                report["timeKind"] = reloadedLatest == null
                    ? "" : reloadedLatest.PlayedAt.Kind.ToString();
                report["latestLocalTime"] = reloadedLatest == null
                    ? "" : reloadedLatest.PlayedAt.ToString(
                        "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                report["accountIsolation"] = accountIsolation;
                report["accountAIsolated"] = accountAIsolated;
                report["accountBIsolated"] = accountBIsolated;
                report["legacyUnassignedPreserved"] = legacyPreserved;
                report["legacyClaimedFromClient"] = legacyClaimed;
                report["claimApplied"] = claimApplied;
                report["accountSelectionPersisted"] =
                    selectedAccountPersisted;
                report["summaryHeroes"] = repository.Summary().Count;
                report["latestChampion"] = latest == null ? "" : latest.ChampionName;
                report["latestPlacement"] = latest == null ? 0 : latest.Placement;
                report["manualAdd"] = manualAdded;
                report["manualUpdate"] = manualUpdated;
                report["manualDelete"] = manualDeleted;
            }
            catch (Exception exception)
            {
                report["success"] = false;
                report["error"] = exception.ToString();
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            File.WriteAllText(
                Path.Combine(outputDirectory, "self-test-result.json"),
                serializer.Serialize(report),
                new UTF8Encoding(false)
            );
        }
    }
}
