# CpSatSpaces

This library adds `IntVar` and `BoolVar` spaces for use with `Google.CpSat` and extends `CpModel` with 
a `SpaceConstraint` builder for conveniently constraining the model.

## Usage

1. Define your dimensions
```csharp
var employees = new Dimension(10, "Employee");
var tasks = new Dimension(50, "Task");
var qualifications = new Dimension(5, "Qualification");

// Space which represents tasks allocated to employees
var taskEmployeeAllocationSpace = new BoolVarSpace(employees, tasks);

// Space which represents employees with qualifications
var employeeQualificationSpace = new BoolVarSpace(employees, qualifications);

// Space which represents tasks which require qualifications
var taskQualificationSpace = new BoolVarSpace(tasks, qualifications);

// Space which represents whether or not an employee has the qualifications for a task
var employeeTaskQualificationSpace = new BoolVarSpace(employees, tasks, qualifications);


var model = new CpModel();

// If an employee does not have a qualification, they WILL BE not qualified for the task
model.AddSpaceConstraint(employeeTaskQualificationSpace)
    .WilleBe(0)
    .If(employeeQualificationAllocation)
    .IsExactly(0);

// If an employee does have a qualification, they CAN BE qualified for the task
model.AddSpaceConstraint()
    .InWhich(employeeTaskQualificationSpace)
    .CanBe(0)
    .If(employeeQualificationAllocation)
    .IsAtleast(1);

// If a task does not require a qualification, an employee WILL BE qualified for it.
model.AddSpaceConstraint()
    .InWhich(employeeTaskQualificationSpace)
    .WillBe(1)
    .If(taskQualificationAllocation)
    .IsExactly(0);

// If a task requires a qualification, an employee CAN BE qualified for it.
model.AddSpaceConstraint()
    .InWhich(employeeTaskQualificationSpace)
    .CanBe(1)
    .If(taskQualificationAllocation)
    .IsAtleast(1);



```


var space = new IntVarSpace(0, 10);
```
