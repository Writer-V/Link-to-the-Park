using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SharpDX.XInput;

namespace LinkToThePark.Input;
public class PlayerInputs
{
    private KeyboardState previousKeyboard = new KeyboardState();
    private KeyboardState currentKeyboard = new KeyboardState();
    private MouseState previousMouse = new MouseState();
    private MouseState currentMouse = new MouseState();
    private GamePadState preveiousGamepad = new GamePadState();
    private GamePadState currentGamepad = new GamePadState();
    private readonly Dictionary<InputAction, InputBinding[]> bindings = new()
    {
        [InputAction.MoveUp]    = new[] {new InputBinding(Keys.Up), new InputBinding(Keys.W), new InputBinding(Buttons.DPadUp), new InputBinding(Buttons.LeftThumbstickUp)},
        [InputAction.MoveDown]  = new[] {new InputBinding(Keys.Down), new InputBinding(Keys.S), new InputBinding(Buttons.DPadDown), new InputBinding(Buttons.LeftThumbstickDown)},
        [InputAction.MoveLeft]  = new[] {new InputBinding(Keys.Left), new InputBinding(Keys.A), new InputBinding(Buttons.DPadLeft), new InputBinding(Buttons.LeftThumbstickLeft)},
        [InputAction.MoveRight] = new[] {new InputBinding(Keys.Right), new InputBinding(Keys.D), new InputBinding(Buttons.DPadRight), new InputBinding(Buttons.LeftThumbstickRight)},
        [InputAction.Attack]    = new[] {new InputBinding(Keys.Space), new InputBinding(MouseButton.Left), new InputBinding(Buttons.X)},
        [InputAction.Confirm]   = new[] {new InputBinding(Keys.Enter), new InputBinding(Buttons.Start)},
        [InputAction.Quit]      = new[] {new InputBinding(Keys.Escape), new InputBinding(Buttons.Back)},
        [InputAction.Debug]     = new[] {new InputBinding(Keys.F9)}
    };
    public IReadOnlyList<InputBinding> GetBindings(InputAction action) => bindings[action];
    private bool IsKeyPressed(InputBinding input)
    {
        return (input.IsDown(currentKeyboard) && !input.IsDown(previousKeyboard)) || (input.IsDown(currentMouse) && !input.IsDown(previousMouse));
    }
    private bool IsKeyReleased(InputBinding input)
    {
        return (!input.IsDown(currentKeyboard) && input.IsDown(previousKeyboard)) || (!input.IsDown(currentMouse) && input.IsDown(previousMouse));
    }
    private bool IsKeyHeld(InputBinding input)
    {
        return input.IsDown(currentKeyboard) || input.IsDown(currentMouse);
    }
    public bool IsAreaClicked(Rectangle area, int screenScale = 1)
    {
        return previousMouse.LeftButton == ButtonState.Pressed && 
            currentMouse.LeftButton == ButtonState.Released
            && area.Contains(currentMouse.Position / screenScale);
    }
    public bool IsActionPressed(InputAction action)
    {
        InputBinding[] actionKeys = bindings[action];
        for (int i = 0; i < actionKeys.Length; i++)
        {
            if (IsKeyPressed(actionKeys[i]))
                return true;
        }
        return false;
    }
    public bool IsActionReleased(InputAction action)
    {
        InputBinding[] actionKeys = bindings[action];
        for (int i = 0; i < actionKeys.Length; i++)
        {
            if (IsKeyReleased(actionKeys[i]))
                return true;
        }
        return false;
    }
    public bool IsActionHeld(InputAction action)
    {
        InputBinding[] actionKeys = bindings[action];
        for (int i = 0; i < actionKeys.Length; i++)
        {
            if (IsKeyHeld(actionKeys[i]))
                return true;
        }
        return false;
    }
    public Point GetMoveAxis()
    //TODO: Implement mouse version based on relative position to player object.
    {
        Point direction = new();
        if(IsActionHeld(InputAction.MoveUp))
            direction.Y -= 1;
        if(IsActionHeld(InputAction.MoveDown))
            direction.Y += 1;
        if(IsActionHeld(InputAction.MoveRight))
            direction.X = 1;
        if (IsActionHeld(InputAction.MoveLeft))
            direction.X -= 1;
        return direction;
    }
    public void UpdateState(bool shouldUpdate)
    {
        previousKeyboard = currentKeyboard;
        previousMouse = currentMouse;
        preveiousGamepad = currentGamepad;
        if (shouldUpdate)
        {
            currentKeyboard = Keyboard.GetState();
            currentMouse = Mouse.GetState();
            currentGamepad = GamePad.GetState(0);
        }
        else
        {
            currentKeyboard = new KeyboardState();
            currentMouse = new MouseState();
        }
    }
    public bool TrySetBindings(InputAction action, params InputBinding[] newBindings)
    {
        ArgumentNullException.ThrowIfNull(newBindings);
        if(newBindings.Length < 1)  //Every action needs at least one key
            return false;
        foreach (KeyValuePair<InputAction,InputBinding[]> assignedKeys in bindings) //Keys can only be used for one thing
        {
            if (assignedKeys.Key != action)
            {
                for(int i = 0; i < newBindings.Length; i++)
                {
                    for(int j = 0; j < assignedKeys.Value.Length; j++)
                    {
                        if (newBindings[i] == assignedKeys.Value[j]) return false;
                    }
                }
            }
        }
        bindings[action] = (InputBinding[])newBindings.Clone(); //You made it this far, we'll take the new bindings.
        return true;
    }
}