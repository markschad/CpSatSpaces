> This project is a work in progress and is not yet ready for production use.

# CpSatSpaces

This library adds `IntVar` and `BoolVar` spaces for use with `Google.CpSat` and extends `CpModel` with 
a `SpaceConstraint` builder for conveniently constraining the model.

## Usage

### 1. Define your dimensions

Use a `Dimensions` record to define the dimensions of your problem.

```csharp
public record Dimensions(
    Dimension Shift,
    Dimension Assignee,
    Dimension Date,
    Dimension Time);

var dimensions = new Dimensions(
    new Dimension("Shift", 20),     // 20 shifts
    new Dimension("Assignee", 10),  // 10 assignees
    new Dimension("Date", 5),       // 5 days
    new Dimension("Time", 96)       // 15 minute time blocks per day
);
```

### 2. Define spaces

Use the dimensions to build variable spaces.

```csharp
// Space which represents the date, time blocks that a shift covers
var shiftDateTimeAllocationSpace = new BoolVarSpace(
    dimensions.Shift,
    dimensions.Date,
    dimensions.Time);

// Space which represents the date, time blocks that an assignee is available
var assigneeDateTimeAvailabilitySpace = new BoolVarSpace(
    dimensions.Assignee,
    dimensions.Date,
    dimensions.Time);

// Space which represents the date, time blocks per shift that an assignee is covering
var assigneeShiftDateTimeAllocationSpace = new BoolVarSpace(
    dimensions.Assignee,
    dimensions.Shift,
    dimensions.Date,
    dimensions.Time);

// Space which represents which assignee is covering a shift
var shiftAssigneeAllocationSpace = new BoolVarSpace(
    dimensions.Shift,
    dimensions.Assignee);
```

### 4. Initialize the variables

```csharp
var myModel = new CpModel();

shiftDateTimeAllocationSpace.InitializeVars(myModel);
assigneeDateTimeAvailabilitySpace.InitializeVars(myModel);
assigneeShiftDateTimeAllocationSpace.InitializeVars(myModel);
shiftAssigneeAllocationSpace.InitializeVars(myModel);
```

### 5. Apply constants

```csharp
// Initialise shift occurence space
foreach (var v in shiftDateTimeAllocationSpace.ValuesAsEnumerable())
{
    var shiftIndex = v.Index[Dimensions.Shift];
    var dateIndex = v.Index[Dimensions.Date];
    var timeIndex = v.Index[Dimensions.Time];
    
    var shift = Configuration.Shifts[shiftIndex];
    var (allocationStartDateTime, allocationEndDateTime) = Configuration.GetDateTimeBlock(dateIndex, timeIndex);
    
    var isShiftOccuring = shift.StartDateTime <= allocationStartDateTime && 
                          shift.EndDateTime >= allocationEndDateTime;

    Add(v.GetValue() == (isShiftOccuring ? 1 : 0));
}

// Initialise assignee availability constant space
foreach (var v in assigneeDateTimeAvailabilitySpace.ValuesAsEnumerable())
{
    var assigneeIndex = v.Index[Dimensions.Assignee];
    var dateIndex = v.Index[Dimensions.Date];
    var timeIndex = v.Index[Dimensions.Time];
    
    var assignee = Configuration.Assignees[assigneeIndex];
    var (blockStartDateTime, blockEndDateTime) = Configuration.GetDateTimeBlock(dateIndex, timeIndex);
    
    var hasAvailability = Configuration.Availabilities
        .Where(a => a.AssigneeId == assignee.Id)
        .Any(availability =>
            availability.StartDateTime <= blockStartDateTime &&
            availability.EndDateTime >= blockEndDateTime);

    Add(v.GetValue() == (hasAvailability ? 1 : 0));
}
```

### 6. Apply constraints

```csharp
// Constrain assignee date, time allocation space such that it can only be allocated if the assignee has
// availability
myModel.AddSpaceConstraint()
    .InWhich(assigneeShiftDateTimeAllocationSpace)
    .CanBeTrue()
    .OnlyIf(assigneeDateTimeAvailabilitySpace)
    .HasTrueCountOfAtLeast(1);  // There may be multiple availabilities for an assignee at a given time


// Constrain the assignee date, time allocation space such that it can only be allocated if there exists a shift
// at that time
myModel.AddSpaceConstraint()
    .InWhich(assigneeShiftDateTimeAllocationSpace)
    .CanBeTrue()
    .OnlyIf(shiftDateTimeAllocationSpace)
    .HasTrueCountOfExactly(1);  // Only one assignee can be allocated to a shift

// Constraint the shift assignee allocation space such that it aligns with the assignee shift date, time
// allocation
myModel.AddSpaceConstraint()
    .InWhich(shiftAssigneeAllocationSpace)
    .WillBeTrue()
    .OnlyIf(assigneeShiftDateTimeAllocationSpace)
    .HasTrueCountOfAtLeast(1);

// Every shift is allocated precisely once
foreach (var shiftSpace in shiftAssigneeAllocationSpace.SubSpacesAsEnumerable([Dimensions.Shift]))
{
    var assigneeAllocations = shiftSpace
        .ValuesAsEnumerable().Select(v => v.GetValue());
    myModel.Add(LinearExpr.Sum(assigneeAllocations) == 1);
}

// If a shift is allocated to an assignee, then each date, time for that shift must also be allocated for the
// same date, time for the assignee
myModel.AddSpaceConstraint()
    .InWhich(assigneeShiftDateTimeAllocationSpace)
    .And(shiftAssigneeAllocationSpace)
    .FormConstraint((_, si, dateTimesPerShiftAssignee, shiftAssignee) =>
    {
        var assigneeIsAssignedShift = shiftAssignee.ValuesAsEnumerable().Single().GetValue();

        var assigneeShiftDateTimes = dateTimesPerShiftAssignee
            .ValuesAsEnumerable()
            .Select(siv => siv.GetValue());

        var shiftDateTimeSpatialIndex = si.Extract(Dimensions.Shift);
        var shiftDateTimes = shiftDateTimeAllocationSpace
            .GetSubSpace(shiftDateTimeSpatialIndex)
            .ValuesAsEnumerable()
            .Select(siv => siv.GetValue());

        myModel.Add(LinearExpr.Sum(shiftDateTimes) == LinearExpr.Sum(assigneeShiftDateTimes))
            .OnlyEnforceIf(assigneeIsAssignedShift);
    });
```
