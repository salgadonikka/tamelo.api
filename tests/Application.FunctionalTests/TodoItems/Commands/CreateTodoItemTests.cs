using Tamelo.Api.Application.Common.Exceptions;
using Tamelo.Api.Application.TodoItems.Commands.CreateTodoItem;
using Tamelo.Api.Application.TodoLists.Commands.CreateTodoList;
using Tamelo.Api.Domain.Entities;

using static Testing;

namespace Tamelo.Api.Application.FunctionalTests.TodoItems.Commands;

public class CreateTodoItemTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        var command = new CreateTodoItemCommand();

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldCreateTodoItem()
    {
        var userId = await RunAsDefaultUserAsync();

        var listId = await SendAsync(new CreateTodoListCommand
        {
            Title = "New List"
        });

        var command = new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "Tasks"
        };

        var itemId = await SendAsync(command);

        var item = await FindAsync<TodoItem>(itemId);

        item.ShouldNotBeNull();
        item!.ListId.ShouldBe(command.ListId);
        item.Title.ShouldBe(command.Title);
        item.CreatedBy.ShouldBe(userId);
        item.Created.ShouldBe(DateTime.Now, TimeSpan.FromMilliseconds(10000));
        item.LastModifiedBy.ShouldBe(userId);
        item.LastModified.ShouldBe(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }
}
