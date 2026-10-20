namespace LinkToThePark;
public enum StoryProgressState
{
    Prologue, // If I add any message before the player can move
    LockedOut, // Standard start
    HasOffering, // Picked up the first one
    HasKey, // Picked up thye second one
    EnteredCastle // Triggered the ending
}