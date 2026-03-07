using System.ComponentModel.DataAnnotations;

namespace _2TDSPR25;

public class Todo
{
    /// <summary>
    /// Id da tarefa. Este campo é gerado automaticamente pelo DB e
    /// não deve ser preenchid na criação de uma nova entidade
    /// </summary>
    public int Id { get;set; }
    public string? Name { get;set; }
    public bool IsComplete {get;set;}
    public string? Secret { get; set; }
}