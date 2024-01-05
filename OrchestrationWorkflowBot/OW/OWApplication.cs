// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace OrchestrationWorkflow.OW
{
    /// <summary>
    /// Data describing a Orchestration Workflow application.
    /// </summary>
    public class OWApplication
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OWApplication"/> class.
        /// </summary>
        /// <param name="projectName">Orchestration Workflow project name.</param>
        /// <param name="deploymentName">Orchestration Workflow model deployment name.</param>
        /// <param name="endpointKey">Orchestration Workflow subscription or endpoint key.</param>
        /// <param name="endpoint">Orchestration Workflow endpoint to use like https://mytextanalyticsresource.cognitive.azure.com.</param>
        public OWApplication(string projectName, string deploymentName, string endpointKey, string endpoint)
            : this((projectName, deploymentName, endpointKey, endpoint))
        {
        }

        private OWApplication(ValueTuple<string, string, string, string> props)
        {
            var (projectName, deploymentName, endpointKey, endpoint) = props;

            if (string.IsNullOrWhiteSpace(projectName))
            {
                throw new ArgumentNullException("projectName value is Null or whitespace. Please use a valid projectName.");
            }

            if (string.IsNullOrWhiteSpace(deploymentName))
            {
                throw new ArgumentNullException("deploymentName value is Null or whitespace. Please use a valid deploymentName.");
            }

            if (string.IsNullOrWhiteSpace(endpointKey))
            {
                throw new ArgumentNullException("endpointKey value is Null or whitespace. Please use a valid endpointKey.");
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentNullException("Endpoint value is Null or whitespace. Please use a valid endpoint.");
            }

            if (!Guid.TryParse(endpointKey, out var _))
            {
                throw new ArgumentException($"\"{endpointKey}\" is not a valid Orchestration Workflow subscription key.");
            }

            if (!Uri.IsWellFormedUriString(endpoint, UriKind.Absolute))
            {
                throw new ArgumentException($"\"{endpoint}\" is not a valid Orchestration Workflow endpoint.");
            }

            ProjectName = projectName;
            DeploymentName = deploymentName;
            EndpointKey = endpointKey;
            Endpoint = endpoint;
        }

        /// <summary>
        /// Gets or sets the Orchestration Workflow project name.
        /// </summary>
        /// <value>
        /// Orchestration Workflow project name.
        /// </value>
        public string ProjectName { get; set; }

        /// <summary>
        /// Gets or sets Orchestration Workflow model deployment name.
        /// </summary>
        /// <value>
        /// Orchestration Workflow model deployment name.
        /// </value>
        public string DeploymentName { get; set; }

        /// <summary>
        /// Gets or sets Orchestration Workflow subscription or endpoint key.
        /// </summary>
        /// <value>
        /// Orchestration Workflow subscription or endpoint key.
        /// </value>
        public string EndpointKey { get; set; }

        /// <summary>
        /// Gets or sets Orchestration Workflow endpoint like https://mytextanalyticsresource.cognitive.azure.com.
        /// </summary>
        /// <value>
        /// Orchestration Workflow endpoint where application is hosted.
        /// </value>
        public string Endpoint { get; set; }
    }
}
