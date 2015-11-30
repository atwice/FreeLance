function onDropdownChangeContractLawFace(dropDownList, contractId) {
    var lawFaceId = dropDownList.options[dropDownList.selectedIndex].value;
    $.ajax({
        url: '@Url.Action("ChangeLawFaceInContract", "Coordinator")',
        type: 'POST',
        data: { "contractId": contractId, "lawFaceId": lawFaceId }
    });
}