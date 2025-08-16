using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoilerApiTests.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;

namespace StorySpoilerApiTests
{
    public class StorySpoilerTests
    {
        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";
        private RestClient _client;
        private static string createdStoryId;

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("at123", "at123at123");

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            _client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });
            var response = client.Execute(request);
            
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [Test, Order(1)]
        public void CreateStory_ShouldReturnCreated()
        {
            var story = new StoryDTO
            {
                Title = "Test Story1",
                Description = "This is a test story1.",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);
            var response = _client.Execute(request);
            

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            createdStoryId = json.GetProperty("storyId").GetString();
            Assert.That(createdStoryId, Is.Not.Null.And.Not.Empty);
            Assert.That(response.Content, Does.Contain("Successfully created!"));
        }

        [Test, Order(2)]

        public void EditStoryTitle_ShouldReturnOk()
        {
            var changes = new StoryDTO
            {
                Title = "Updated Test Story1",
                Description = "This is an updated test story1.",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);

            request.AddJsonBody(changes);
            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }

        [Test, Order(3)]

        public void GetAllStory_ShouldReturnList()
        {
            var request = new RestRequest($"/api/Story/All", Method.Get);

            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(json, Is.Not.Null.And.Not.Empty);
        }

        [Test, Order(4)]

        public void DeleteStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));

        }

        [Test, Order(5)]

        public void CreateStoryWithInvalidData_ShouldReturnBadRequest()
        {
            var story = new StoryDTO
            {
                Title = "",
                Description = "",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }

        [Test, Order(6)]

        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            string nonExistingStoryId = "12345";
            var changes = new StoryDTO
            {
                Title = "123",
                Description = "123",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{nonExistingStoryId}", Method.Put);

            request.AddJsonBody(changes);
            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Test, Order(7)]

        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            string nonExistingStoryId = "123";

            var request = new RestRequest($"/api/Story/Delete/{nonExistingStoryId}", Method.Delete);
            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _client?.Dispose();
        }
    }
}