// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace OrchestrationWorkflow.OW
{
    public class OWEntity
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("confidenceScore")]
        public float ConfidenceScore { get; set; }

        [JsonProperty("displayOrder")]
        public int DisplayOrder { get; set; }
        [JsonProperty("qnaId")]
        public int QnAId { get; set; }

        [JsonProperty("displayText")]
        public string  DisplayText { get; set; }

    }
}
