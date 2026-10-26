using System;
using System.Collections.Generic;
using UnityEngine;

public enum RoomDirection
{
    North,
    East,
    South,
    West
}

[Serializable]
public class VastuMessage
{
    public bool   isUser;
    public string text;
}

/// <summary>
/// VastuAssistantManager
/// =====================
/// Central data store for the Vastu chat.
/// Keeps a message list and raises OnMessageAdded so any UI (VastuChatView,
/// VastuDashboardView) can react without polling.
///
/// InjectApiResults() is called by VastuApiClient after a successful
/// /analyze response and renders every detection as a chat message.
/// </summary>
public class VastuAssistantManager : MonoBehaviour
{
    public static VastuAssistantManager Instance { get; private set; }

    public event Action<VastuMessage> OnMessageAdded;

    readonly List<VastuMessage> messages = new List<VastuMessage>();

    [SerializeField] RoomDirection selectedDirection = RoomDirection.North;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public accessors ──────────────────────────────────────────────────────
    public IReadOnlyList<VastuMessage> GetMessages()   => messages;
    public RoomDirection               SelectedDirection => selectedDirection;

    public void EnsureWelcomeMessage()
    {
        if (messages.Count > 0)
            return;

        AddMessage(new VastuMessage
        {
            isUser = false,
            text   = "Namaste! I'm your CoZee Vastu assistant.\n" +
                     "• Tap \"Scan with AI\" to analyse the room in front of you.\n" +
                     "• Set the direction selector to match your room's actual compass facing.\n" +
                     "• Ask me about bed placement, kitchen zones, or any furniture layout."
        });
    }

    public void SetDirection(RoomDirection direction)
    {
        selectedDirection = direction;
    }

    /// <summary>
    /// Adds a user message and generates a simple rule-based reply.
    /// The reply is intentionally brief — the AI scan provides the deep analysis.
    /// </summary>
    public void SendUserMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        AddMessage(new VastuMessage { isUser = true, text = text.Trim() });
        AddMessage(new VastuMessage
        {
            isUser = false,
            text   = BuildAssistantReply(text.Trim(), selectedDirection)
        });
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    void AddMessage(VastuMessage message)
    {
        messages.Add(message);
        OnMessageAdded?.Invoke(message);
    }

    string BuildAssistantReply(string userText, RoomDirection direction)
    {
        string dir   = direction.ToString();
        string lower = userText.ToLowerInvariant();

        if (lower.Contains("bed"))
            return $"For a {dir}-facing bedroom, place the bed's headboard toward the south or east wall when possible.";

        if (lower.Contains("kitchen"))
            return $"The ideal kitchen zone is the southeast corner. Your room faces {dir} — keep cooking and fire elements southeast.";

        if (lower.Contains("living") || lower.Contains("sofa") || lower.Contains("couch"))
            return $"In a {dir}-facing room, seat heavy furniture (sofa, sectional) along the south or west walls. Keep the northeast light and uncluttered.";

        if (lower.Contains("mirror"))
            return "Mirrors are best on north or east walls. Avoid placing them directly facing the bed (south wall).";

        if (lower.Contains("wardrobe") || lower.Contains("cupboard"))
            return $"Wardrobes and heavy storage suit the south-west corner. In a {dir}-facing room, check that the door opens toward the north or east.";

        if (lower.Contains("scan") || lower.Contains("analys"))
            return "Tap the \"Scan with AI\" button — I'll identify every piece of furniture in the room and score its Vastu placement instantly.";

        return $"Considering your room faces {dir}: keep the northeast open and uncluttered, place heavy furniture in the south-west, and ensure good light at the main entrance.";
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  API result injection
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Called by VastuApiClient after a successful POST /analyze response.
    /// Converts the structured response into human-readable chat messages.
    ///
    /// NOTE ON image_zone:
    ///   The backend direction.py maps pixel position within the photo to a
    ///   compass label (e.g. "West" = left side of photo).  This is NOT a
    ///   hardware compass reading.  We label it "photo zone" in the UI and
    ///   show the server's own direction_note as a disclaimer.
    /// </summary>
    public void InjectApiResults(VastuApiResponse result)
    {
        if (result == null)
        {
            AddMessage(new VastuMessage
            {
                isUser = false,
                text   = "⚠ AI scan returned an empty result. Try again with a well-lit room photo."
            });
            return;
        }

        // ── Header ────────────────────────────────────────────────────────
        string scoreEmoji = result.overall_score >= 80 ? "🟢" :
                            result.overall_score >= 50 ? "🟡" : "🔴";

        AddMessage(new VastuMessage
        {
            isUser = false,
            text   = $"━━ AI Room Scan — {result.room_type} ━━\n" +
                     $"{scoreEmoji} Overall Vastu score: {result.overall_score} / 100\n" +
                     result.summary
        });

        // ── Per-item results ──────────────────────────────────────────────
        if (result.vastu_results != null && result.vastu_results.Length > 0)
        {
            foreach (VastuResultItem item in result.vastu_results)
            {
                // Find the matching detection for confidence.
                float conf = 0f;
                if (result.detections != null)
                {
                    foreach (VastuDetectionItem det in result.detections)
                    {
                        if (string.Equals(det.name, item.furniture,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            conf = det.confidence;
                            break;
                        }
                    }
                }

                string statusEmoji = item.status == "Excellent" ? "✅" :
                                     item.status == "Poor"      ? "❌" : "⚠";
                int confPct = Mathf.RoundToInt(conf * 100f);

                // photo zone, NOT compass — see direction_note below.
                string line = $"{statusEmoji} {CapFirst(item.furniture)}" +
                              (confPct > 0 ? $" ({confPct}% confidence)" : "") +
                              $"\n   Photo zone: {item.image_zone}" +
                              $" | {item.status} ({item.score}/100)" +
                              $"\n   💡 {item.suggestion}";

                AddMessage(new VastuMessage { isUser = false, text = line });
            }
        }
        else
        {
            AddMessage(new VastuMessage
            {
                isUser = false,
                text   = "No furniture was detected with sufficient confidence. " +
                         "Try a brighter, less cluttered frame and tap Scan again."
            });
        }

        // ── Direction note disclaimer (always shown) ──────────────────────
        // The server's direction_note field explains the image_zone convention.
        string note = !string.IsNullOrEmpty(result.direction_note)
            ? result.direction_note
            : "Note: 'photo zone' shows where furniture sits within your photo, not an absolute compass direction.";

        AddMessage(new VastuMessage
        {
            isUser = false,
            text   = "ℹ️ " + note + "\nUse the N/E/S/W selector above to set your room's real compass facing."
        });
    }

    // ── Utility ───────────────────────────────────────────────────────────────
    static string CapFirst(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);
}
