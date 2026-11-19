using CleanArchitecture.Application;
using CleanArchitecture.Domain;
using CleanArchitecture.Infrastructure;
using Xunit;

namespace CleanArchitecture.Tests;

public class ArchitectureAndDomainTests
{
    [Fact]
    public void Empty_order_cannot_be_confirmed()
    {
        var order = new Order(Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => order.Confirm());
    }

    [Fact]
    public void Confirmed_order_cannot_be_modified()
    {
        var order = new Order(Guid.NewGuid());
        order.AddItem("BOOK", 25m, 2);
        order.Confirm();
        Assert.Equal(50m, order.Total);
        Assert.Throws<InvalidOperationException>(() => order.AddItem("PEN", 1m, 1));
    }

    [Fact]
    public async Task Use_case_works_without_a_web_server_or_database()
    {
        IOrderRepository repository = new InMemoryOrderRepository();
        var id = await new CreateOrder(repository).ExecuteAsync("BOOK", 25m, 2);
        await new ConfirmOrder(repository).ExecuteAsync(id);
        Assert.True((await repository.GetAsync(id))!.IsConfirmed);
    }

    [Fact]
    public void Domain_and_application_do_not_reference_outer_projects()
    {
        var domainRefs = typeof(Order).Assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();
        var appRefs = typeof(ConfirmOrder).Assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();

        Assert.DoesNotContain(domainRefs, name => name is not null &&
            (name.StartsWith("CleanArchitecture.Application") ||
             name.StartsWith("CleanArchitecture.Infrastructure") ||
             name.StartsWith("CleanArchitecture.Api") ||
             name.StartsWith("Microsoft.EntityFrameworkCore") ||
             name.StartsWith("Microsoft.AspNetCore")));
        Assert.DoesNotContain(appRefs, name => name is not null &&
            (name.StartsWith("CleanArchitecture.Infrastructure") ||
             name.StartsWith("CleanArchitecture.Api") ||
             name.StartsWith("Microsoft.EntityFrameworkCore") ||
             name.StartsWith("Microsoft.AspNetCore")));
    }
}
