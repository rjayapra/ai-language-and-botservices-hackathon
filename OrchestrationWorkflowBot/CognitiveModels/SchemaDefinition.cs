// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using OrchestrationWorkflow.OW;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata.Ecma335;

namespace OrchestrationWorkflow
{
    /// <summary>
    /// An <see cref="IRecognizerConvert"/> implementation that provides helper methods and properties to interact with
    /// the recognizer results.
    /// </summary>
    public class SchemaDefinition : IRecognizerConvert
    {
        public enum Intent
        {
            BookFlight,
            Cancel,
            GetWeather,
            Surface_QA,
            BookMeeting,
            CancelMeeting,
            UpdateMeeting,
            ViewCalendar,
            RespondMeetingRequest,
            None
        }

        public string Text { get; set; }

        public string AlteredText { get; set; }

        public Dictionary<Intent, IntentScore> Intents { get; set; }

        public OWEntities Entities { get; set; }

        public IDictionary<string, object> Properties { get; set; }

        private static string NormalizeIntent(string intent)
        {
            return intent.Replace('.', '_').Replace(' ', '_');
        }

        public void Convert(dynamic result)
        {
            var jsonResult = JsonConvert.SerializeObject(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Console.Write(jsonResult.ToString());
          
            var app = JsonConvert.DeserializeObject<SchemaDefinition>(jsonResult);

            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
        }

        public (Intent intent, double score) GetTopIntent()
        {
            var maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }

            return (maxIntent, max);
        }

        public class OWEntities
        {
            public OWEntity[] Entities;

            public OWEntity[] GetFromCityList() => Entities.Where(e => e.Category == "fromCity").ToArray();

            public OWEntity[] GetToCityList() => Entities.Where(e => e.Category == "toCity").ToArray();

            public OWEntity[] GetFlightDateList() => Entities.Where(e => e.Category == "flightDate").ToArray();

            public string GetFromCity() => GetFromCityList().FirstOrDefault()?.Text;

            public string GetToCity() => GetToCityList().FirstOrDefault()?.Text;

            public string GetFlightDate() => GetFlightDateList().FirstOrDefault()?.Text;


            public OWEntity[] GetAttendantList() => Entities.Where(e => e.Category == "Attendants").ToArray();

            public OWEntity[] GetMeetingDateList() => Entities.Where(e => e.Category == "Date").ToArray();

            public OWEntity[] GetLocationList() => Entities.Where(e => e.Category == "Location").ToArray();

            public string GetAttendant() => GetAttendantList().FirstOrDefault()?.Text;

            public string GetMeetingDate() => GetMeetingDateList().FirstOrDefault()?.Text;

            public string GetLocation() => GetLocationList().FirstOrDefault()?.Text;

            public OWEntity[] GetPromptsList() => Entities.Where(e => e.DisplayOrder >= 0).ToArray();
            
            public string[] GetPromptText() => GetPromptsList().Select(e => e.DisplayText).ToArray();
            
            
        }
    }
}
