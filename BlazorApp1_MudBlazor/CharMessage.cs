//CharMessage.cs
using RosbridgeNet.RosbridgeClient.Common.Attributes;

[RosMessageType("std_msgs/String")]
public class CharMessage
{
    public string? data { get; set; } // Thuộc tính data của thông điệp
}
