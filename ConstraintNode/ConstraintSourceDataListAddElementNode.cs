using Warudo.Core.Attributes;

namespace Warudo.Plugins.Core.Nodes;

[NodeType(
    Id = "mira-constraint-source-list-add-element",
    Title = "Constraint Source Add Element",
    Category = "Unity Constraints"
)]
public class ConstraintSourceDataListAddElementNode : ListAddElementNode<ConstraintSourceData> { }
