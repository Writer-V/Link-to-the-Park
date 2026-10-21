namespace LinkToThePark.Graphics;
public enum AnimSequenceType
{
    Default, //For when nothing applies
    Idle, IdleLeft, IdleRight, IdleUp,
    MoveLeft, MoveRight, MoveUp, MoveDown, 
    Attack, AttackLeft, AttackRight, AttackUp,
    Pose, Hit, Death, //Characters
    RegularAttack, PowerAttack, ToxicAttack, EvilAttack, SpiritAttack, //Destructive animations
    Explosion, DustImpact, //Effects
    Creation //On creation, before regular animations
}