function onDropdownChangeContractLawFace(dropDownList, contractId) {
    var lawFaceId = dropDownList.options[dropDownList.selectedIndex].value;
    $.ajax({
        url: '@Url.Action("ChangeLawFaceInContract", "Coordinator")',
        type: 'POST',
        data: { "contractId": contractId, "lawFaceId": lawFaceId }
    });
}

function onDropdownChangeProblemLawFace(dropDownList, problemId) {
    var lawFaceId = dropDownList.options[dropDownList.selectedIndex].value;
    $.ajax({
        url: '@Url.Action("ChangeLawFaceInProblem", "Coordinator")',
        type: 'POST',
        data: { "problemId": problemId, "lawFaceId": lawFaceId }
    });
}