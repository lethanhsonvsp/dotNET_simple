using Opc.Ua.Server;
using Opc.Ua;

public class MyNodeManager : CustomNodeManager2
{
    private BaseDataVariableState? waterFlowRateVariable;
    private BaseDataVariableState? pumpTemperatureVariable;
    private BaseDataVariableState? pumpStatusVariable; // Thêm biến trạng thái của bơm

    public MyNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        : base(server, configuration, "http://yourorganization.org/SimpleOpcUaServer/")
    {
    }

    public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
    {
        base.CreateAddressSpace(externalReferences);

        // Tạo một folder
        var folder = new FolderState(null)
        {
            SymbolicName = "MyObjects",
            ReferenceTypeId = ReferenceTypeIds.Organizes,
            TypeDefinitionId = ObjectTypeIds.FolderType,
            NodeId = new NodeId("MyObjects", NamespaceIndex),
            BrowseName = new QualifiedName("MyObjects", NamespaceIndex),
            DisplayName = new LocalizedText("MyObjects"),
            Description = new LocalizedText("MyObjects"),
            WriteMask = AttributeWriteMask.None,
            UserWriteMask = AttributeWriteMask.None
        };

        AddPredefinedNode(SystemContext, folder);

        // Tạo biến lưu lượng nước
        waterFlowRateVariable = CreateVariable(folder, "WaterFlowRate", "Water Flow Rate", BuiltInType.Double, ValueRanks.Scalar);
        waterFlowRateVariable.Value = new Variant(0.0);
        waterFlowRateVariable.StatusCode = StatusCodes.Good;
        waterFlowRateVariable.Timestamp = DateTime.UtcNow;

        AddPredefinedNode(SystemContext, waterFlowRateVariable);

        // Tạo biến nhiệt độ của bơm
        pumpTemperatureVariable = CreateVariable(folder, "PumpTemperature", "Pump Temperature", BuiltInType.Double, ValueRanks.Scalar);
        pumpTemperatureVariable.Value = new Variant(30.0); // Giá trị ban đầu là 30 độ C
        pumpTemperatureVariable.StatusCode = StatusCodes.Good;
        pumpTemperatureVariable.Timestamp = DateTime.UtcNow;

        AddPredefinedNode(SystemContext, pumpTemperatureVariable);

        // Tạo biến trạng thái của bơm
        pumpStatusVariable = CreateVariable(folder, "PumpStatus", "Pump Status", BuiltInType.String, ValueRanks.Scalar);
        pumpStatusVariable.Value = new Variant("N"); // Giá trị ban đầu là 'N'
        pumpStatusVariable.StatusCode = StatusCodes.Good;
        pumpStatusVariable.Timestamp = DateTime.UtcNow;

        AddPredefinedNode(SystemContext, pumpStatusVariable);
    }

    private BaseDataVariableState CreateVariable(NodeState parent, string name, string description, BuiltInType dataType, int valueRank)
    {
        var variable = new BaseDataVariableState(parent)
        {
            SymbolicName = name,
            ReferenceTypeId = ReferenceTypeIds.HasComponent,
            TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
            NodeId = new NodeId(name, NamespaceIndex),
            BrowseName = new QualifiedName(name, NamespaceIndex),
            DisplayName = new LocalizedText(name),
            Description = new LocalizedText(description),
            WriteMask = AttributeWriteMask.None,
            UserWriteMask = AttributeWriteMask.None,
            DataType = (uint)dataType,
            ValueRank = valueRank,
            AccessLevel = AccessLevels.CurrentReadOrWrite,
            UserAccessLevel = AccessLevels.CurrentReadOrWrite,
            Historizing = false
        };

        return variable;
    }
}
