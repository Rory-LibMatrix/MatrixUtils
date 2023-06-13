using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MatrixRoomUtils.Core.Extensions;

namespace MatrixRoomUtils.Core.Responses;

public class CreateRoomRequest {
    [JsonIgnore] public CreationContentBaseType _creationContentBaseType;

    public CreateRoomRequest() => _creationContentBaseType = new CreationContentBaseType(this);

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("room_alias_name")]
    public string RoomAliasName { get; set; } = null!;

    //we dont want to use this, we want more control
    // [JsonPropertyName("preset")]
    // public string Preset { get; set; } = null!;
    [JsonPropertyName("initial_state")]
    public List<StateEvent> InitialState { get; set; } = null!;

    [JsonPropertyName("visibility")]
    public string Visibility { get; set; } = null!;

    [JsonPropertyName("power_level_content_override")]
    public PowerLevelEvent PowerLevelContentOverride { get; set; } = null!;

    [JsonPropertyName("creation_content")]
    public JsonObject CreationContent { get; set; } = new();

    /// <summary>
    ///     For use only when you can't use the CreationContent property
    /// </summary>

    public StateEvent this[string event_type, string event_key = ""] {
        get => InitialState.First(x => x.Type == event_type && x.StateKey == event_key);
        set {
            var stateEvent = InitialState.FirstOrDefault(x => x.Type == event_type && x.StateKey == event_key);
            if (stateEvent == null)
                InitialState.Add(value);
            else
                InitialState[InitialState.IndexOf(stateEvent)] = value;
        }
    }

    //extra properties
    [JsonIgnore]
    public string HistoryVisibility {
        get {
            var stateEvent = InitialState.FirstOrDefault(x => x.Type == "m.room.history_visibility");
            if (stateEvent == null) {
                InitialState.Add(new StateEvent {
                    Type = "m.room.history_visibility",
                    Content = new JsonObject {
                        ["history_visibility"] = "shared"
                    }
                });
                return "shared";
            }

            return stateEvent.ContentAsJsonNode["history_visibility"].GetValue<string>();
        }
        set {
            var stateEvent = InitialState.FirstOrDefault(x => x.Type == "m.room.history_visibility");
            if (stateEvent == null)
                InitialState.Add(new StateEvent {
                    Type = "m.room.history_visibility",
                    Content = new JsonObject {
                        ["history_visibility"] = value
                    }
                });
            else {
                var v = stateEvent.ContentAsJsonNode;
                v["history_visibility"] = value;
                stateEvent.ContentAsJsonNode = v;
            }
        }
    }

    [JsonIgnore]
    public string RoomIcon {
        get {
            var stateEvent = InitialState.FirstOrDefault(x => x.Type == "m.room.avatar");
            if (stateEvent == null) {
                InitialState.Add(new StateEvent {
                    Type = "m.room.avatar",
                    Content = new JsonObject {
                        ["url"] = ""
                    }
                });
                return "";
            }

            return stateEvent.ContentAsJsonNode["url"].GetValue<string>();
        }
        set {
            var stateEvent = InitialState.FirstOrDefault(x => x.Type == "m.room.avatar");
            if (stateEvent == null)
                InitialState.Add(new StateEvent {
                    Type = "m.room.avatar",
                    Content = new JsonObject {
                        ["url"] = value
                    }
                });
            else {
                var v = stateEvent.ContentAsJsonNode;
                v["url"] = value;
                stateEvent.ContentAsJsonNode = v;
            }
        }
    }

    // [JsonIgnore]
    // public string GuestAccess
    // {
    //     get
    //     {
    //         var stateEvent = InitialState.FirstOrDefault(x => x.Type == "m.room.guest_access");
    //         if (stateEvent == null)
    //         {
    //             InitialState.Add(new StateEvent()
    //             {
    //                 Type = "m.room.guest_access",
    //                 Content = new JsonObject()
    //                 {
    //                     ["guest_access"] = "can_join"
    //                 }
    //             });
    //             return "can_join";
    //         }
    //
    //         return stateEvent.ContentAsJsonNode["guest_access"].GetValue<string>();
    //     }
    //     set
    //     {
    //         var stateEvent = InitialState.FirstOrDefault(x => x.Type == "m.room.guest_access");
    //         if (stateEvent == null)
    //         {
    //             InitialState.Add(new StateEvent()
    //             {
    //                 Type = "m.room.guest_access",
    //                 Content = new JsonObject()
    //                 {
    //                     ["guest_access"] = value
    //                 }
    //             });
    //         }
    //         else
    //         {
    //             var v = stateEvent.ContentAsJsonNode;
    //             v["guest_access"] = value;
    //             stateEvent.ContentAsJsonNode = v;
    //         }
    //     }
    // }

    public ServerACL ServerACLs {
        get {
            var stateEvent = InitialState.FirstOrDefault(x => x.Type == "m.room.server_acl");
            if (stateEvent == null) {
                InitialState.Add(new StateEvent {
                    Type = "m.room.server_acl",
                    Content = new JsonObject {
                        ["allow"] = new JsonArray {
                            "*"
                        },
                        ["deny"] = new JsonArray()
                    }
                });
                return new ServerACL {
                    Allow = new List<string> {
                        "*"
                    },
                    Deny = new List<string>(),
                    AllowIpLiterals = true
                };
            }

            return new ServerACL {
                Allow = stateEvent.ContentAsJsonNode["allow"].Deserialize<List<string>>(),
                Deny = stateEvent.ContentAsJsonNode["deny"].Deserialize<List<string>>(),
                AllowIpLiterals = true
            };
        }
        set {
            Console.WriteLine($"Setting server acl to {value.ToJson()}");
            var stateEvent = InitialState.FirstOrDefault(x => x.Type == "m.room.server_acl");
            if (stateEvent == null)
                InitialState.Add(new StateEvent {
                    Type = "m.room.server_acl",
                    Content = new JsonObject {
                        ["allow"] = JsonNode.Parse(JsonSerializer.Serialize(value.Allow)),
                        ["deny"] = JsonNode.Parse(JsonSerializer.Serialize(value.Deny))
                            ["allow_ip_literals"] = value.AllowIpLiterals
                    }
                });
            else {
                var v = stateEvent.ContentAsJsonNode;
                v["allow"] = JsonNode.Parse(JsonSerializer.Serialize(value.Allow));
                v["deny"] = JsonNode.Parse(JsonSerializer.Serialize(value.Deny));
                v["allow_ip_literals"] = value.AllowIpLiterals;
                stateEvent.ContentAsJsonNode = v;
                Console.WriteLine($"v={v.ToJson()}");
                Console.WriteLine($"stateEvent.ContentAsJsonNode={stateEvent.ContentAsJsonNode.ToJson()}");
            }
        }
    }

    public Dictionary<string, string> Validate() {
        Dictionary<string, string> errors = new();
        if (!Regex.IsMatch(RoomAliasName, @"[a-zA-Z0-9_\-]+$"))
            errors.Add("room_alias_name", "Room alias name must only contain letters, numbers, underscores, and hyphens.");

        return errors;
    }
}

public class CreationContentBaseType {
    private readonly CreateRoomRequest createRoomRequest;

    public CreationContentBaseType(CreateRoomRequest createRoomRequest) => this.createRoomRequest = createRoomRequest;

    [JsonPropertyName("type")]
    public string Type {
        get => (string)createRoomRequest.CreationContent["type"];
        set {
            if (value is "null" or "") createRoomRequest.CreationContent.Remove("type");
            else createRoomRequest.CreationContent["type"] = value;
        }
    }
}

public class PowerLevelEvent {
    [JsonPropertyName("ban")]
    public int Ban { get; set; } // = 50;

    [JsonPropertyName("events_default")]
    public int EventsDefault { get; set; } // = 0;

    [JsonPropertyName("events")]
    public Dictionary<string, int> Events { get; set; } // = null!;

    [JsonPropertyName("invite")]
    public int Invite { get; set; } // = 50;

    [JsonPropertyName("kick")]
    public int Kick { get; set; } // = 50;

    [JsonPropertyName("notifications")]
    public NotificationsPL NotificationsPl { get; set; } // = null!;

    [JsonPropertyName("redact")]
    public int Redact { get; set; } // = 50;

    [JsonPropertyName("state_default")]
    public int StateDefault { get; set; } // = 50;

    [JsonPropertyName("users")]
    public Dictionary<string, int> Users { get; set; } // = null!;

    [JsonPropertyName("users_default")]
    public int UsersDefault { get; set; } // = 0;
}

public class NotificationsPL {
    [JsonPropertyName("room")]
    public int Room { get; set; } = 50;
}

public class ServerACL {
    [JsonPropertyName("allow")]
    public List<string> Allow { get; set; } // = null!;

    [JsonPropertyName("deny")]
    public List<string> Deny { get; set; } // = null!;

    [JsonPropertyName("allow_ip_literals")]
    public bool AllowIpLiterals { get; set; } // = false;
}