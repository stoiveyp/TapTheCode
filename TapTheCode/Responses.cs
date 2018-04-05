using Alexa.NET.Response;

namespace TapTheCode
{
    public class Responses
    {
        public const string Success = "That's great, I found the button to use as the lock";
        public const string TimedOut = "I'm sorry, but I didn't recognise any buttons being pressed. Without a button I can't start the game. To try again please start another game.";
        public const string Unknown = "Sorry - but I didn't recognise that request";
        public const string Welcome = "Welcome to Tap The Code. This game requires an echo button. Please press the button you wish to use for this game.";
    }
}