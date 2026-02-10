namespace BooksPortal.Application.Features.BulkImport.Interfaces;

public interface IImportTemplateService
{
    byte[] CreateBooksTemplate();
    byte[] CreateTeachersTemplate();
    byte[] CreateStudentsTemplate();
}
