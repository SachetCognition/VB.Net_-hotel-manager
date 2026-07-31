using Xunit;

namespace HotelManagement.Tests.UI;

/// <summary>
/// Category 11: UI State Tests (~30 tests)
/// Tests UI state logic (button enable/disable, form reset, grid selection)
/// without requiring actual WinForms controls.
/// </summary>
public class UiStateTests
{
    // Simulates the form state machine used across all VB.NET forms
    private enum FormMode { Browse, Add, Edit }

    private class FormState
    {
        public FormMode Mode { get; set; } = FormMode.Browse;
        public bool BtnSaveEnabled => Mode == FormMode.Add || Mode == FormMode.Edit;
        public bool BtnDeleteEnabled => Mode == FormMode.Browse && HasSelection;
        public bool BtnNewEnabled => Mode == FormMode.Browse;
        public bool BtnUpdateEnabled => Mode == FormMode.Browse && HasSelection;
        public bool GridEnabled => Mode == FormMode.Browse;
        public bool FieldsEnabled => Mode != FormMode.Browse;
        public bool HasSelection { get; set; }
        public Dictionary<string, string> Fields { get; set; } = new();

        public void Reset()
        {
            Fields.Clear();
            HasSelection = false;
            Mode = FormMode.Browse;
        }

        public void SelectRow(Dictionary<string, string> rowData)
        {
            Fields = new Dictionary<string, string>(rowData);
            HasSelection = true;
        }

        public void ClickNew()
        {
            Fields.Clear();
            HasSelection = false;
            Mode = FormMode.Add;
        }

        public void ClickSave()
        {
            Mode = FormMode.Browse;
        }

        public void ClickEdit()
        {
            if (HasSelection) Mode = FormMode.Edit;
        }
    }

    // ============= BUTTON STATE TESTS =============

    // TC-UI-001: Initial state - Save disabled
    [Fact]
    public void InitialState_SaveDisabled()
    {
        var state = new FormState();
        Assert.False(state.BtnSaveEnabled);
    }

    // TC-UI-002: Initial state - New enabled
    [Fact]
    public void InitialState_NewEnabled()
    {
        var state = new FormState();
        Assert.True(state.BtnNewEnabled);
    }

    // TC-UI-003: Initial state - Delete disabled (no selection)
    [Fact]
    public void InitialState_DeleteDisabled()
    {
        var state = new FormState();
        Assert.False(state.BtnDeleteEnabled);
    }

    // TC-UI-004: After New click - Save enabled
    [Fact]
    public void AfterNew_SaveEnabled()
    {
        var state = new FormState();
        state.ClickNew();
        Assert.True(state.BtnSaveEnabled);
    }

    // TC-UI-005: After New click - New disabled
    [Fact]
    public void AfterNew_NewDisabled()
    {
        var state = new FormState();
        state.ClickNew();
        Assert.False(state.BtnNewEnabled);
    }

    // TC-UI-006: After Save - back to Browse mode
    [Fact]
    public void AfterSave_BackToBrowse()
    {
        var state = new FormState();
        state.ClickNew();
        state.ClickSave();
        Assert.Equal(FormMode.Browse, state.Mode);
    }

    // TC-UI-007: After Save - Save disabled
    [Fact]
    public void AfterSave_SaveDisabled()
    {
        var state = new FormState();
        state.ClickNew();
        state.ClickSave();
        Assert.False(state.BtnSaveEnabled);
    }

    // TC-UI-008: After row selection - Delete enabled
    [Fact]
    public void AfterRowSelection_DeleteEnabled()
    {
        var state = new FormState();
        state.SelectRow(new Dictionary<string, string> { { "ID", "1" } });
        Assert.True(state.BtnDeleteEnabled);
    }

    // TC-UI-009: After row selection - Update enabled
    [Fact]
    public void AfterRowSelection_UpdateEnabled()
    {
        var state = new FormState();
        state.SelectRow(new Dictionary<string, string> { { "ID", "1" } });
        Assert.True(state.BtnUpdateEnabled);
    }

    // TC-UI-010: During Edit - fields enabled
    [Fact]
    public void DuringEdit_FieldsEnabled()
    {
        var state = new FormState();
        state.SelectRow(new Dictionary<string, string> { { "ID", "1" } });
        state.ClickEdit();
        Assert.True(state.FieldsEnabled);
    }

    // ============= FORM RESET TESTS =============

    // TC-UI-011: Reset clears all fields
    [Fact]
    public void Reset_ClearsAllFields()
    {
        var state = new FormState();
        state.Fields["Name"] = "Test";
        state.Fields["Address"] = "123 St";
        state.Reset();
        Assert.Empty(state.Fields);
    }

    // TC-UI-012: Reset removes selection
    [Fact]
    public void Reset_RemovesSelection()
    {
        var state = new FormState();
        state.SelectRow(new Dictionary<string, string> { { "ID", "1" } });
        state.Reset();
        Assert.False(state.HasSelection);
    }

    // TC-UI-013: Reset goes to Browse mode
    [Fact]
    public void Reset_GoesToBrowseMode()
    {
        var state = new FormState();
        state.ClickNew();
        state.Reset();
        Assert.Equal(FormMode.Browse, state.Mode);
    }

    // TC-UI-014: New click clears fields
    [Fact]
    public void ClickNew_ClearsFields()
    {
        var state = new FormState();
        state.Fields["Name"] = "Test";
        state.ClickNew();
        Assert.Empty(state.Fields);
    }

    // TC-UI-015: New click removes selection
    [Fact]
    public void ClickNew_RemovesSelection()
    {
        var state = new FormState();
        state.SelectRow(new Dictionary<string, string> { { "ID", "1" } });
        state.ClickNew();
        Assert.False(state.HasSelection);
    }

    // ============= GRID SELECTION TESTS =============

    // TC-UI-016: Grid row selection populates fields
    [Fact]
    public void GridRowSelection_PopulatesFields()
    {
        var state = new FormState();
        var rowData = new Dictionary<string, string>
        {
            { "GuestID", "G-123456" },
            { "GuestName", "John Doe" },
            { "Address", "123 Main St" }
        };
        state.SelectRow(rowData);
        Assert.Equal("G-123456", state.Fields["GuestID"]);
        Assert.Equal("John Doe", state.Fields["GuestName"]);
    }

    // TC-UI-017: Second selection replaces first
    [Fact]
    public void SecondSelection_ReplacesFirst()
    {
        var state = new FormState();
        state.SelectRow(new Dictionary<string, string> { { "Name", "First" } });
        state.SelectRow(new Dictionary<string, string> { { "Name", "Second" } });
        Assert.Equal("Second", state.Fields["Name"]);
    }

    // TC-UI-018: Grid disabled during Add mode
    [Fact]
    public void GridDisabled_DuringAddMode()
    {
        var state = new FormState();
        state.ClickNew();
        Assert.False(state.GridEnabled);
    }

    // TC-UI-019: Grid enabled in Browse mode
    [Fact]
    public void GridEnabled_InBrowseMode()
    {
        var state = new FormState();
        Assert.True(state.GridEnabled);
    }

    // TC-UI-020: Grid disabled during Edit mode
    [Fact]
    public void GridDisabled_DuringEditMode()
    {
        var state = new FormState();
        state.SelectRow(new Dictionary<string, string> { { "ID", "1" } });
        state.ClickEdit();
        Assert.False(state.GridEnabled);
    }

    // ============= MODE TRANSITION TESTS =============

    // TC-UI-021: Browse -> Add transition
    [Fact]
    public void Transition_BrowseToAdd()
    {
        var state = new FormState();
        Assert.Equal(FormMode.Browse, state.Mode);
        state.ClickNew();
        Assert.Equal(FormMode.Add, state.Mode);
    }

    // TC-UI-022: Add -> Browse transition (Save)
    [Fact]
    public void Transition_AddToBrowse()
    {
        var state = new FormState();
        state.ClickNew();
        state.ClickSave();
        Assert.Equal(FormMode.Browse, state.Mode);
    }

    // TC-UI-023: Browse -> Edit transition
    [Fact]
    public void Transition_BrowseToEdit()
    {
        var state = new FormState();
        state.SelectRow(new Dictionary<string, string> { { "ID", "1" } });
        state.ClickEdit();
        Assert.Equal(FormMode.Edit, state.Mode);
    }

    // TC-UI-024: Edit -> Browse transition (Save)
    [Fact]
    public void Transition_EditToBrowse()
    {
        var state = new FormState();
        state.SelectRow(new Dictionary<string, string> { { "ID", "1" } });
        state.ClickEdit();
        state.ClickSave();
        Assert.Equal(FormMode.Browse, state.Mode);
    }

    // TC-UI-025: Edit not possible without selection
    [Fact]
    public void Edit_NotPossible_WithoutSelection()
    {
        var state = new FormState();
        state.ClickEdit(); // No selection
        Assert.Equal(FormMode.Browse, state.Mode); // Should stay in Browse
    }

    // ============= FIELD ENABLE/DISABLE =============

    // TC-UI-026: Fields disabled in Browse
    [Fact]
    public void Fields_DisabledInBrowse()
    {
        var state = new FormState();
        Assert.False(state.FieldsEnabled);
    }

    // TC-UI-027: Fields enabled in Add
    [Fact]
    public void Fields_EnabledInAdd()
    {
        var state = new FormState();
        state.ClickNew();
        Assert.True(state.FieldsEnabled);
    }

    // TC-UI-028: Fields enabled in Edit
    [Fact]
    public void Fields_EnabledInEdit()
    {
        var state = new FormState();
        state.SelectRow(new Dictionary<string, string> { { "ID", "1" } });
        state.ClickEdit();
        Assert.True(state.FieldsEnabled);
    }

    // TC-UI-029: Save disabled without New or Edit
    [Fact]
    public void Save_DisabledWithoutNewOrEdit()
    {
        var state = new FormState();
        state.SelectRow(new Dictionary<string, string> { { "ID", "1" } });
        Assert.False(state.BtnSaveEnabled); // Browse mode, not Add/Edit
    }

    // TC-UI-030: Delete only in Browse with selection
    [Fact]
    public void Delete_OnlyInBrowseWithSelection()
    {
        var state = new FormState();
        Assert.False(state.BtnDeleteEnabled); // No selection
        state.SelectRow(new Dictionary<string, string> { { "ID", "1" } });
        Assert.True(state.BtnDeleteEnabled); // Has selection
        state.ClickNew();
        Assert.False(state.BtnDeleteEnabled); // Not in Browse anymore
    }
}
