using System.Text.Json.Serialization;

namespace WhiteboardProjectBuilder.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "itemType")]
[JsonDerivedType(typeof(ProjectItem), "project")]
[JsonDerivedType(typeof(SomedayMaybePair), "somedayMaybe")]
public interface IWhiteboardItem
{
    DateTime CreatedDate { get; set; }
    bool IsArchived { get; set; }
}
