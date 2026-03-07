using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http.Metadata;

namespace _2TDSPR25;

[Serializable]
public class TodoItemDTO 
{
// {: IEndpointParameterMetadataProvider, IBindableFromHttpContext<TodoItemDTO>
// {
//     public static void PopulateMetadata(
//         ParameterInfo parameter,
//         EndpointBuilder builder)
//     {
//         builder.Metadata.Add(
//             new AcceptsMetadata(
//                 ["application/json", "text/json", "application/xml", "text/xml"],
//                 typeof(Todo)
//             )
//         );
//     }
//     
//     public static async ValueTask<TodoItemDTO?> BindAsync(
//         HttpContext context, 
//         ParameterInfo parameter)
//     {
//         var xmlDoc = await XDocument.LoadAsync(context.Request.Body, LoadOptions.None, context.RequestAborted);
//         var serializer = new XmlSerializer(typeof(Todo));
//         return (TodoItemDTO?)serializer.Deserialize(xmlDoc.CreateReader());
//     }

    [property:Description("Id da tarefa")]
    public int Id { get;set; }
    [property:Description("Nome da tarefa")]
    [property:MinLength(3)]
    [property:MaxLength(255)]
    [property:DefaultValue("Tarefa sem nome")]
    public string? Name { get;set; }

    [property:Description("Status de conclusão da tarefa")]
    public bool IsComplete { get; set; }

    [property:Description("Dia da semana para completar a tarefa")]
    public DayOfWeekAsString DayOfWeekToComplete { get; set; }

    public TodoItemDTO() { }
    public TodoItemDTO(Todo todoItem) => (Id, Name, IsComplete) = 
        (todoItem.Id, todoItem.Name, todoItem.IsComplete);
}