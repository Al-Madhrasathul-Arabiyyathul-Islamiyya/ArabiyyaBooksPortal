using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.Settings.DTOs;
using BooksPortal.Application.Features.Settings.Services;
using BooksPortal.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace BooksPortal.UnitTests.Features.Settings;

public class SlipTemplateSettingServiceTests
{
    private readonly IRepository<SlipTemplateSetting> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SlipTemplateSettingService _service;

    public SlipTemplateSettingServiceTests()
    {
        _repository = Substitute.For<IRepository<SlipTemplateSetting>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _service = new SlipTemplateSettingService(_repository, _unitOfWork);
    }

    [Fact]
    public void GetDefaultLabels_ReturnsNonEmptyList()
    {
        var labels = SlipTemplateSettingService.GetDefaultLabels();
        labels.Should().NotBeEmpty();
    }

    [Fact]
    public void GetDefaultLabels_ContainsAllCategories()
    {
        var labels = SlipTemplateSettingService.GetDefaultLabels();
        var categories = labels.Select(l => l.Category).Distinct().ToList();

        categories.Should().Contain("Common");
        categories.Should().Contain("Distribution");
        categories.Should().Contain("Return");
        categories.Should().Contain("TeacherIssue");
        categories.Should().Contain("TeacherReturn");
    }

    [Fact]
    public void GetDefaultLabels_CommonCategory_HasSchoolName()
    {
        var labels = SlipTemplateSettingService.GetDefaultLabels();
        var schoolName = labels.FirstOrDefault(l => l.Category == "Common" && l.Key == "SchoolName");

        schoolName.Should().NotBeNull();
        schoolName!.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void GetDefaultLabels_EachCategory_HasSortOrderStartingAtZero()
    {
        var labels = SlipTemplateSettingService.GetDefaultLabels();
        var grouped = labels.GroupBy(l => l.Category);

        foreach (var group in grouped)
        {
            group.Min(l => l.SortOrder).Should().Be(0,
                $"category '{group.Key}' should have sort order starting at 0");
        }
    }

    [Fact]
    public void GetDefaultLabels_NoDuplicateCategoryKeyPairs()
    {
        var labels = SlipTemplateSettingService.GetDefaultLabels();
        var pairs = labels.Select(l => $"{l.Category}:{l.Key}").ToList();

        pairs.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(99).ReturnsNull();

        var act = () => _service.GetByIdAsync(99);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsResponse()
    {
        var entity = new SlipTemplateSetting
        {
            Id = 1,
            Category = "Common",
            Key = "SchoolName",
            Value = "Test School",
            SortOrder = 0
        };
        _repository.GetByIdAsync(1).Returns(entity);

        var result = await _service.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.Category.Should().Be("Common");
        result.Key.Should().Be("SchoolName");
        result.Value.Should().Be("Test School");
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(99).ReturnsNull();

        var act = () => _service.UpdateAsync(99, new UpdateSlipTemplateSettingRequest { Value = "X", SortOrder = 0 });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_Found_UpdatesValueAndSortOrder()
    {
        var entity = new SlipTemplateSetting
        {
            Id = 1,
            Category = "Common",
            Key = "SchoolName",
            Value = "Old",
            SortOrder = 0
        };
        _repository.GetByIdAsync(1).Returns(entity);

        var result = await _service.UpdateAsync(1, new UpdateSlipTemplateSettingRequest { Value = "New", SortOrder = 5 });

        result.Value.Should().Be("New");
        result.SortOrder.Should().Be(5);
        _repository.Received(1).Update(entity);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public void GetDefaultLabels_DistributionCategory_HasTitle()
    {
        var labels = SlipTemplateSettingService.GetDefaultLabels();
        var title = labels.FirstOrDefault(l => l.Category == "Distribution" && l.Key == "Title");

        title.Should().NotBeNull();
        title!.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void GetDefaultLabels_AllValuesNonEmpty()
    {
        var labels = SlipTemplateSettingService.GetDefaultLabels();

        foreach (var label in labels)
        {
            label.Value.Should().NotBeNullOrEmpty(
                $"label '{label.Category}:{label.Key}' should have a non-empty value");
        }
    }
}
