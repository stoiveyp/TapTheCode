using System.Collections.Generic;
using System.Linq;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Alexa.NET.StateManagement;
using Alexa.NET.Gadgets.GameEngine.Directives;
using Alexa.NET.Gadgets.GadgetController;
using Alexa.NET.Gadgets.GameEngine.Requests;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace TapTheCode
{
    public class Function
    {
        public Function()
        {
            new Alexa.NET.Gadgets.GameEngine.Requests.GadgetRequestHandler().AddToRequestConverter();
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            var state = input.StateManagement();

            switch (input.Request)
            {
                case LaunchRequest _:
                    var response = ResponseBuilder.Tell(Responses.Welcome);
                    response.Response.ShouldEndSession = null;
                    SetAllToPurple(response);
                    AddRollCall(response);
                    return response;
                case InputHandlerEventRequest inputHandler:
                    var respondingEvent = inputHandler.Events.First();
                    if (respondingEvent.Name == EventNames.Complete)
                    {
                        state.SetSession("lockGadget",respondingEvent.InputEvents.First().GadgetId);
                        var successResponse = ResponseBuilder.Tell(Responses.Success);
                        successResponse.Response.ShouldEndSession = null;
                        AddSuccess(successResponse, state.GetSession<string>("lockGadget"));
                        return successResponse;
                    }

                    return ResponseBuilder.Tell(Responses.TimedOut);
            }

            return ResponseBuilder.Tell(Responses.Unknown);
        }

        private void AddSuccess(SkillResponse response, string gadgetId)
        {
            var directive = new SetLightDirective();
            directive.TargetGadgets.Add(gadgetId);

            directive.Parameters = new SetLightParameter
            {
                Animations = new List<SetLightAnimation>(new[]
                {
                    new SetLightAnimation
                    {
                        TargetLights = new List<string>{"1"},
                        Sequence = new List<AnimationSegment>
                        {
                            new AnimationSegment
                            {
                                Color="00FF00",
                                DurationMilliseconds = 10000
                            }
                        }
                    }
                })
            };

            response.Response.Directives.Add(directive);
        }

        private void SetAllToPurple(SkillResponse response)
        {
            response.GadgetColor("800080", new string[] { }, 10000);
        }

        private void AddRollCall(SkillResponse response)
        {
            var pressedButton = new PatternRecognizer
            {
                Actions = new List<string> { PatternAction.Down },
                Anchor = PatternRecognizerAnchor.Start,
                Patterns = new List<Pattern>
                {
                    new Pattern
                    {
                        Repeat = 1,
                        Action = ButtonAction.Down,
                        GadgetIds = new List<string> {"lock"}
                    }
                }
            };

            var directive = new StartInputHandlerDirective
            {
                TimeoutMilliseconds = 10000,
                Proxies = new List<string> { "lock" }
            };
            directive.Events.Add(EventNames.Complete,new InputHandlerEvent
            {
                Meets = new List<string> { "found lock"},
                Reports = GadgetEventReportType.Matches,
                EndInputHandler = true
            });
            directive.Events.Add(EventNames.Failed, new InputHandlerEvent
            {
                Meets = new List<string> { "timed out" },
                Reports = GadgetEventReportType.Matches,
                EndInputHandler = true
            });

            directive.Recognizers.Add("found lock", pressedButton);
            response.Response.Directives.Add(directive);
        }
    }

    internal class EventNames
    {
        public static string Complete = "complete";
        public static string Failed = "failed";
    }
}
