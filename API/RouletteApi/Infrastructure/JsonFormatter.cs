using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RouletteApi.Infrastructure
{
    public class JsonTextFormatter : ITextFormatter
    {
        // the cached host name which is part of every log message
        private readonly string hostname;

        public JsonTextFormatter()
        {
            hostname = Dns.GetHostName();
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            try
            {
                // flatten the message as a simpler option to look at it within the RenderedMessage property of the JSON file
                var renderedMessage = logEvent.RenderMessage();

                // create the message object
                var message = new { logEvent.MessageTemplate, logEvent.Properties, RenderedMessage = renderedMessage, Level = logEvent.Level.ToString(), Exception = logEvent.Exception?.ToString(), Hostname = hostname };

                // serialize the object as JSON
                output.Write(JsonConvert.SerializeObject(message));
            }
            catch (Exception exception)
            {
                try
                {
                    var message = new { RenderedMessage = "Failed to render log message.", MessageTemplate = logEvent.MessageTemplate.Text, Exception = exception.ToString() };
                    output.Write(JsonConvert.SerializeObject(message));
                }
                catch (Exception ex)
                {
                    // fallback to just return the fact that we were unable to render the log message
                    output.Write($"Unable to render log message. Reason was {ex}");
                }
            }
        }
    }
}
