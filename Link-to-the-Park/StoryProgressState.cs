namespace LinkToThePark;
public enum StoryProgressState
{
    Prologue, // If I add any message before the player can move
    LockedOut, // Standard start
    HasOffering, // Picked up the first one
    HasKey, // Picked up thye second one
    GateOpens, // First ending state
    BackAway, // PC backs away
    OtherWalksOut, // Non-played PC walks out
    CharMessage, // PC speaks
    OtherMessage // Other char speaks, the Victory
}