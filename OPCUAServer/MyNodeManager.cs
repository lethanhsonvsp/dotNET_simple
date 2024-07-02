using Opc.Ua.Server;
using Opc.Ua;

public class MyNodeManager : CustomNodeManager2
{

    private BaseDataVariableState? helloVariable; // Thêm biến char

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

        // Tạo biến trạng thái của bơm
        helloVariable = CreateVariable(folder, "Hello", "Hello Status", BuiltInType.String, ValueRanks.Scalar);
        helloVariable.Value = new Variant("N"); // Giá trị ban đầu là 'N'
        helloVariable.StatusCode = StatusCodes.Good;
        helloVariable.Timestamp = DateTime.UtcNow;

        AddPredefinedNode(SystemContext, helloVariable);
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
