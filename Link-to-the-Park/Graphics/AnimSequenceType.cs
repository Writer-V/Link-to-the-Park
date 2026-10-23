namespace LinkToThePark.Graphics;
public enum AnimSequenceType
{
    Default, //For when nothing applies
    //Characters
    Idle, IdleLeft, IdleRight, IdleUp,
    MoveLeft, MoveRight, MoveUp, MoveDown,
    Attack, AttackLeft, AttackRight, AttackUp,
    Pose, PoseLeft, PoseRight, PoseUp,
    Hit, HitLeft, HitRight, HitUp,
    Death, DeathLeft, DeathRight, DeathUp,
    RegularAttack, PowerAttack, ToxicAttack, EvilAttack, SpiritAttack, //Destructive animations
    Explosion, DustImpact, //Effects
    Creation //On creation, before regular animations
}