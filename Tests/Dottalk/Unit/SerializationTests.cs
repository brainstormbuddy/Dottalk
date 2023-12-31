using System;
using System.Collections.Generic;
using System.Linq;
using Dottalk.App.Domain.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Tests.Dottalk.Support;
using Xunit;

namespace Tests.Dottalk.Unit
{
    public class SerializationTests
    {
        public SerializationTests()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
        }

        [Fact(DisplayName = "Should serialize a connection object properly")]
        public void TestShouldSerializerConnectionObject()
        {
            // arrange
            var chatRoomId = Guid.NewGuid();
            var serverInstanceId1 = Guid.NewGuid();
            var serverInstanceId2 = Guid.NewGuid();
            var chatRoomConnectionPool = TestingScenarioBuilder
                .BuildChatRoomConnectionPoolTwoInstances(chatRoomId, 6, serverInstanceId1, serverInstanceId2);

            // act -- serializes a complex object using the app settings
            var serializedChatRoomConnectionPoolStr = JsonConvert.SerializeObject(chatRoomConnectionPool);

            // assert - guarantees camelCase and not PascalCase on the jsons
            var jsonChatRoomConnection = JObject.Parse(serializedChatRoomConnectionPoolStr);
            var serverInstances = (JArray)jsonChatRoomConnection["serverInstances"];

            Assert.Equal(chatRoomId.ToString(), jsonChatRoomConnection["chatRoomId"]);
            Assert.Equal(4, jsonChatRoomConnection["totalActiveConnections"].Value<int>());
            Assert.Equal(2, serverInstances.Count);
            Assert.Equal(serverInstanceId1.ToString(),
                jsonChatRoomConnection["serverInstances"][0]["serverInstanceId"].Value<string>());
            Assert.Equal(serverInstanceId2.ToString(),
                jsonChatRoomConnection["serverInstances"][1]["serverInstanceId"].Value<string>());
        }

        [Fact(DisplayName = "Should deserialize a connection object properly")]
        public void TestShouldDeserializeAConnectionObject()
        {
            // arrange
            var chatRoomConnectionPoolStr = @"
            {
                'chatRoomId': '6576b56b-8c96-47d0-b17f-70ef3d84bfda',
                'activeConnectionsLimit': 6,
                'totalActiveConnections': 5,
                'serverInstances': [
                    {   
                        'serverInstanceId': '4945c63c-0e8b-4fb2-810b-5c8071091e04', 
                        'connectedUsers': [
                            {
                               'UserId':'1c101959-b95e-4c08-bd69-a2e1ee759646',
                               'ConnectionId' :'connection 1'
                            },
                            {
                               'UserId':'8e9404e6-ba38-4118-9a19-0060314be702',
                               'ConnectionId' :'connection 2'
                            },
                            {
                               'UserId':'1f5ee50d-6e78-4ed7-bc65-4b69ada9de8a',
                               'ConnectionId' :'connection 3'
                            }
                        ]
                    },
                    {
                        'serverInstanceId': 'c4bf9cba-815e-4b20-a23b-fd404ab6fa15',
                        'connectedUsers': [
                            {
                               'UserId': '37bb6e69-167a-4a52-b14d-442d6ba27871',
                               'ConnectionId' :'connection 4'
                            },
                            {
                               'UserId': '9fd878fc-f3bf-4bb0-903c-0d81627b8412',
                               'ConnectionId' :'connection 5'
                            }
                        ]
                    }
                ]
            }";

            // act
            var deserializedConnectionPool = JsonConvert.DeserializeObject<ChatRoomConnectionPool>(chatRoomConnectionPoolStr);

            // assert
            Assert.Equal("6576b56b-8c96-47d0-b17f-70ef3d84bfda", deserializedConnectionPool.ChatRoomId.ToString());
            Assert.Equal(6, deserializedConnectionPool.ActiveConnectionsLimit);
            Assert.Equal(5, deserializedConnectionPool.TotalActiveConnections);

            // first instance asserts
            Assert.Equal("4945c63c-0e8b-4fb2-810b-5c8071091e04", deserializedConnectionPool.ServerInstances.First().ServerInstanceId.ToString());
            Assert.Equal(3, deserializedConnectionPool.ServerInstances.First().ConnectedUsers.Count());
            Assert.Equal("1c101959-b95e-4c08-bd69-a2e1ee759646", deserializedConnectionPool.ServerInstances.First().ConnectedUsers.First().UserId.ToString());
            Assert.Equal("1f5ee50d-6e78-4ed7-bc65-4b69ada9de8a", deserializedConnectionPool.ServerInstances.First().ConnectedUsers.ElementAt(2).UserId.ToString());
            Assert.Equal("connection 3", deserializedConnectionPool.ServerInstances.First().ConnectedUsers.ElementAt(2).ConnectionId);

            // second instance asserts
            Assert.Equal("c4bf9cba-815e-4b20-a23b-fd404ab6fa15", deserializedConnectionPool.ServerInstances.ElementAt(1).ServerInstanceId.ToString());
            Assert.Equal(2, deserializedConnectionPool.ServerInstances.ElementAt(1).ConnectedUsers.Count());
            Assert.Equal("37bb6e69-167a-4a52-b14d-442d6ba27871", deserializedConnectionPool.ServerInstances.ElementAt(1).ConnectedUsers.First().UserId.ToString());
            Assert.Equal("9fd878fc-f3bf-4bb0-903c-0d81627b8412", deserializedConnectionPool.ServerInstances.ElementAt(1).ConnectedUsers.ElementAt(1).UserId.ToString());
        }
    }
}