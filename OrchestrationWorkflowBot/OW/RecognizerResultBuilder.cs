// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;
using System;

namespace OrchestrationWorkflow.OW
{
    /// <summary>
    /// A helper class that creates and populate <see cref="RecognizerResult"/> from a <see cref="AnalyzeConversationResult"/> instance.
    /// </summary>
    internal static class RecognizerResultBuilder
    {
        private const string MetadataKey = "$instance";

        private static readonly HashSet<string> DateSubtypes = new HashSet<string>
        {
            "date",
            "daterange",
            "datetime",
            "datetimerange",
            "duration",
            "set",
            "time",
            "timerange"
        };

        private static readonly HashSet<string> GeographySubtypes = new HashSet<string>
        {
            "poi",
            "city",
            "countryRegion",
            "continent",
            "state"
        };

        public static RecognizerResult BuildRecognizerResultFromOWResponse(JsonDocument owResult, string utterance)
        {

            JsonElement orchestrationTaskResult = owResult.RootElement;
            JsonElement orchestrationPrediction = orchestrationTaskResult.GetProperty("result").GetProperty("prediction");
            
            var recognizerResult = new RecognizerResult
            {
                Text = utterance,
                AlteredText = orchestrationTaskResult.GetProperty("result").GetProperty("query").GetString()
            };
            string ans = "NA";
            string topIntent = orchestrationPrediction.GetProperty("topIntent").GetString();
            JsonElement targetIntentResult = orchestrationPrediction.GetProperty("intents").GetProperty(topIntent);
            
            //Conversational language understanding response
            if (targetIntentResult.GetProperty("targetProjectKind").GetString() == "Conversation")
            {
                JsonElement conversationPrediction = targetIntentResult.GetProperty("result").GetProperty("prediction");
                UpdateRecognizerResultFromConversations(conversationPrediction, recognizerResult);                    
                AddProperties(conversationPrediction, recognizerResult);
            }
            else if (targetIntentResult.GetProperty("targetProjectKind").GetString() == "QuestionAnswering")
            {
                
                JsonElement questionAnsweringResponse = targetIntentResult.GetProperty("result");
                
                foreach (JsonElement answer in questionAnsweringResponse.GetProperty("answers").EnumerateArray())
                {
                    Console.WriteLine($"\t\t{answer.GetProperty("answer").GetString()}");
                    Console.WriteLine($"\t\tConfidence: {answer.GetProperty("confidenceScore")}");
                    Console.WriteLine($"\t\tSource: {answer.GetProperty("source")}");
                    Console.WriteLine();  
                    ans = answer.GetProperty("answer").GetString();
                    break;
                                        
                }
                recognizerResult = new RecognizerResult
                {
                    Text = ans,
                    AlteredText = orchestrationTaskResult.GetProperty("result").GetProperty("query").GetString()
                };
                UpdateRecognizerResultFromQA(topIntent, questionAnsweringResponse, recognizerResult);
                AddProperties(orchestrationPrediction, recognizerResult);
            }
            //var owecognizerResult = new OWRecognizerResult(recognizerResult,ans);
            return recognizerResult;          

        }

        /// <summary>
        /// Returns a RecognizerResult from a conversations project response.
        /// 
        /// Intents: List of Intents with their confidence scores.
        /// Entities: has the object: { "entities" : [{entity1}, {entity2}] }
        /// Properties: Additional information returned by the service.
        /// 
        /// </summary>
        private static void UpdateRecognizerResultFromConversations(JsonElement conversationPrediction, RecognizerResult recognizerResult)
        {
            recognizerResult.Intents = GetConversationIntents(conversationPrediction);
            recognizerResult.Entities = ExtractConversationEntitiesAndMetadata(conversationPrediction);            
                        
        }

        private static void UpdateRecognizerResultFromQA(String topIntent, JsonElement conversationPrediction, RecognizerResult recognizerResult)
        {
            recognizerResult.Intents = GetQAIntents(topIntent, conversationPrediction);
            recognizerResult.Entities = ExtractQAEntitiesAndMetadata(conversationPrediction);           
        }
        /*
         * Eg: 
           "intents": {
            "Book_Update_Cancel_Respond_Meeting_ViewCalendar": {
            "confidenceScore": 0.8754766,
            "targetProjectKind": "Conversation",
            "result": {
            "query": "Book a meeting with Joh on Wednesday between 2-3PM",
            "prediction": {
                "topIntent": "UpdateMeeting",
                "projectKind": "Conversation",
                "intents": [
                {
                    "category": "BookMeeting",
                    "confidenceScore": 0.9999999
                },
                ...              
                ],
                "entities": [
                {
                    "category": "Decide",
                    "text": "Book",
                    "offset": 0,
                    "length": 4,
                    "confidenceScore": 1
                },
         */
        private static IDictionary<string, IntentScore> GetConversationIntents(JsonElement prediction)
        {
            var result = new Dictionary<string, IntentScore>();
            foreach (var intent in prediction.GetProperty("intents").EnumerateArray())
            {
                result.Add(intent.GetProperty("category").GetString(), new IntentScore {Score = intent.GetProperty("confidenceScore").GetSingle()});
            }
            return result;
        }

        private static IDictionary<string, IntentScore> GetQAIntents(string topIntent, JsonElement prediction)
        {
            var result = new Dictionary<string, IntentScore>();
            IntentScore score = new IntentScore();            
            foreach (JsonElement answer in prediction.GetProperty("answers").EnumerateArray())
            {
                score = new IntentScore { Score = answer.GetProperty("confidenceScore").GetSingle() };
                break;
            }
            result.Add(topIntent, score);
            return result;
        }

        private static JObject ExtractConversationEntitiesAndMetadata(JsonElement prediction)
        {
            var entities = prediction.GetProperty("entities").GetRawText(); // Requires refactoring
            //var entityObject = JsonConvert.SerializeObject(entities);
            var jsonArray = JArray.Parse(entities);
            var returnedObject = new JObject { {"entities", jsonArray } };

            return returnedObject;
        }
        private static JObject ExtractQAEntitiesAndMetadata(JsonElement prediction)
        {
            var returnedObject = new JObject { };
            foreach (JsonElement answer in prediction.GetProperty("answers").EnumerateArray())
            {
                //Console.WriteLine($"\t\tSource: {answer.GetProperty("source")}");
               // Console.WriteLine($"\t\tAnswer: {answer}");
               // Console.WriteLine($"\t\tPrediction: {prediction}");
                var prompts = answer.GetProperty("dialog").GetProperty("prompts").GetRawText();
                var jsonArray = JArray.Parse(prompts);
                returnedObject = new JObject { { "entities", jsonArray } };
                break;
            }
            return returnedObject;
        }

        private static void AddProperties(JsonElement conversationPrediction, RecognizerResult result)
        {
            var topIntent = conversationPrediction.GetProperty("topIntent").GetString();
            var projectKind = conversationPrediction.GetProperty("projectKind").GetString();

            result.Properties.Add("projectKind", projectKind.ToString());

            if (topIntent != null)
            {
                result.Properties.Add("topIntent", topIntent);
            }
        }

        private static IDictionary<string, IntentScore> GetIntents(JObject cluResult)
        {
            var result = new Dictionary<string, IntentScore>();
            var intents = (JObject)cluResult["intents"];
            if (intents != null)
            {
                foreach (var intent in intents)
                {
                    result.Add(NormalizeIntent(intent.Key), new IntentScore {Score = intent.Value["score"]?.Value<double>() ?? 0.0});
                }
            }

            return result;
        }

        private static string NormalizeIntent(string intent)
        {
            return intent.Replace('.', '_').Replace(' ', '_');
        }

        private static string NormalizeEntity(string entity)
        {
            // Type::Role -> Role
            var type = entity.Split(':').Last();
            return type.Replace('.', '_').Replace(' ', '_');
        }

        private static JToken MapProperties(JToken source, bool inInstance)
        {
            var result = source;
            if (source is JObject obj)
            {
                var nobj = new JObject();

                // Fix datetime by reverting to simple timex
                if (!inInstance && obj.TryGetValue("type", out var type) && type.Type == JTokenType.String && DateSubtypes.Contains(type.Value<string>()))
                {
                    var timexs = obj["values"];
                    var arr = new JArray();
                    if (timexs != null)
                    {
                        var unique = new HashSet<string>();
                        foreach (var elt in timexs)
                        {
                            unique.Add(elt["timex"]?.Value<string>());
                        }

                        foreach (var timex in unique)
                        {
                            arr.Add(timex);
                        }

                        nobj["timex"] = arr;
                    }

                    nobj["type"] = type;
                }
                else
                {
                    // Map or remove properties
                    foreach (var property in obj.Properties())
                    {
                        var name = NormalizeEntity(property.Name);
                        var isArray = property.Value.Type == JTokenType.Array;
                        var isString = property.Value.Type == JTokenType.String;
                        var isInt = property.Value.Type == JTokenType.Integer;
                        var val = MapProperties(property.Value, inInstance || property.Name == MetadataKey);
                        if (name == "datetime" && isArray)
                        {
                            nobj.Add("datetimeV1", val);
                        }
                        else if (name == "datetimeV2" && isArray)
                        {
                            nobj.Add("datetime", val);
                        }
                        else if (inInstance)
                        {
                            // Correct $instance issues
                            if (name == "length" && isInt)
                            {
                                nobj.Add("endIndex", property.Value.Value<int>() + property.Parent["startIndex"].Value<int>());
                            }
                            else if (!((isInt && name == "modelTypeId") ||
                                       (isString && name == "role")))
                            {
                                nobj.Add(name, val);
                            }
                        }
                        else
                        {
                            // Correct non-$instance values
                            if (name == "unit" && isString)
                            {
                                nobj.Add("units", val);
                            }
                            else
                            {
                                nobj.Add(name, val);
                            }
                        }
                    }
                }

                result = nobj;
            }
            else if (source is JArray arr)
            {
                var narr = new JArray();
                foreach (var elt in arr)
                {
                    // Check if element is geographyV2
                    var isGeographyV2 = string.Empty;
                    foreach (var props in elt.Children())
                    {
                        var tokenProp = props as JProperty;
                        if (tokenProp == null)
                        {
                            break;
                        }

                        if (tokenProp.Name.Contains("type") && GeographySubtypes.Contains(tokenProp.Value.ToString()))
                        {
                            isGeographyV2 = tokenProp.Value.ToString();
                            break;
                        }
                    }

                    if (!inInstance && !string.IsNullOrEmpty(isGeographyV2))
                    {
                        var geoEntity = new JObject();
                        foreach (var props in elt.Children())
                        {
                            var tokenProp = (JProperty)props;
                            if (tokenProp.Name.Contains("value"))
                            {
                                geoEntity.Add("location", tokenProp.Value);
                            }
                        }

                        geoEntity.Add("type", isGeographyV2);
                        narr.Add(geoEntity);
                    }
                    else
                    {
                        narr.Add(MapProperties(elt, inInstance));
                    }
                }

                result = narr;
            }

            return result;
        }
    }
}
