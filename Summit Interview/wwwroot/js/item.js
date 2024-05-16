var make_select;
var selectedFiles = [];
var pagination = {
	filterByItemName: "",
	filterByItemCategory: "",
	page: 1,
	limit: 10,
	totalPage: 0,
	totalItem: 0,
};

window.addEventListener("dragover", function (e) {
	e.preventDefault();
}, false);
window.addEventListener("drop", function (e) {
	e.preventDefault();
}, false);

$(document).ready(() => {

	loadItems();

	make_select = new TomSelect("#category", {
		create: false,
		load: function (query, callback) {
			var url = '/Category/GetCategory?name=' + encodeURIComponent(query);
			fetch(url)
				.then(response => response.json())
				.then(json => {
					if (json.status === 200) {
						callback(json.data.map(data => ({
							value: data.id,
							text: data.name,
						})));
					}
					callback();
				}).catch(() => {
					callback();
				});
		},
	});

	document.getElementById("custom-file-upload").addEventListener('drop', (e) => {
		e.preventDefault();
		e.stopPropagation();

		selectedFiles.push(...e.dataTransfer.files);

		previewImage();

	}, false);
})


$("#fileUploadForm").on("submit", (e) => {
	e.preventDefault();

	if (file === null) {
		toastr.error("File cannot be empty", "Error");
		return;
	}

	const payload = new FormData();
	payload.append("file", file);

	$.ajax({
		url: "/Item/BulkUpload",
		type: "POST",
		data: payload,
		processData: false,
		contentType: false,
		success: (result) => {
			if (result.status === 201) {
				toastr.success(result.message, "Success");
				file = null;
				document.getElementById("file_input").value = null;
				$("#previewFile").html("");
				$("#bulkUploadModal").modal("toggle");
				loadItems();
			} else if (result.status >= 500) {
				toastr.error(result.message, "Error");
			} else {
				toastr.warning(result.message, "Error");
			}
		},
		error: (error) => {
			console.log(error);
		}
	})
})


$("#customFileUploadBtn").on("click", (e) => {
	e.stopPropagation();
	$("#item_images").click();
})

$("#item_images").on("change", (e) => {
	selectedFiles.push(...e.currentTarget.files);
	document.getElementById("item_images").value = null;
	previewImage();
})

function previewImage() {
	if (selectedFiles.length) {
		let htmlString = "";
		for (let i = 0; i < selectedFiles.length; i++) {
			htmlString += `
				<div class="position-relative">
					<img class="img-thumbnail"
						style="height:100px;width:100px;padding:0px;margin-right:5px;margin-bottom:10px;"
						src="${URL.createObjectURL(selectedFiles[i])}" />
					<div onclick="removeImage(${i})" class="close-btn">
						<i class="bi bi-x-circle-fill"></i>
					</div>
				</div>
			`
		}

		$("#previewRow").html(htmlString);
	}
}

function removeImage(index) {
	selectedFiles.splice(index, 1);
	previewImage();
}

$("#itemCrudForm").on("submit", (e) => {
	e.preventDefault();

	const formData = new FormData(e.currentTarget);
	const formProps = Object.fromEntries(formData);

	const payload = new FormData();
	payload.append("categoryId", formProps.Category || "");
	payload.append("itemName", formProps.ItemName || "");
	payload.append("itemUnit", formProps.ItemUnit || "");
	payload.append("itemQuantity", formProps.ItemQuantity || "");
	payload.append("itemDescription", tinymce.activeEditor.getContent());

	for (var i = 0; i != selectedFiles.length; i++) {
		payload.append("files", selectedFiles[i]);
	}

	$.ajax({
		url: "/Item/CreateItem",
		type: "POST",
		data: payload,
		processData: false,
		contentType: false,
		success: (result) => {
			if (result.status === 201) {
				toastr.success(result.message, "Success");
				$("#itemCrudModal").modal("toggle");
				loadItems();
			} else if (result.status === 410) {
				console.log(result);
				if (Object.keys(result.message).includes("ItemName") && result.message?.ItemName?.errors?.length) {
					$("#item_name").addClass("is-invalid");
					$("#item_name_error").html(`*${result.message?.ItemName?.errors?.[0]?.errorMessage}`);
				}
				if (Object.keys(result.message).includes("ItemUnit") && result.message?.ItemUnit?.errors?.length) {
					$("#item_unit").addClass("is-invalid");
					$("#item_unit_error").html(`*${result.message?.ItemUnit?.errors?.[0]?.errorMessage}`);
				}
				if (Object.keys(result.message).includes("ItemQuantity") && result.message?.ItemQuantity?.errors?.length) {
					$("#item_quantity").addClass("is-invalid");
					$("#item_quantity_error").html(`*${result.message?.ItemQuantity?.errors?.[0]?.errorMessage}`);
				}
				if (Object.keys(result.message).includes("CategoryId") && result.message?.CategoryId?.errors?.length) {
					$("#category").addClass("is-invalid");
					$(".ts-wrapper").addClass("is-invalid");
					$("#category_error").html(`*${result.message?.CategoryId?.errors?.[0]?.errorMessage}`);
				}
				if (Object.keys(result.message).includes("ItemImages") && result.message?.ItemImages?.errors?.length) {
					$("#item_images_error").html(`*${result.message?.ItemImages?.errors?.[0]?.errorMessage}`);
				}
			} else if (result.status >= 500) {
				toastr.error(result.message, "Error");
			} else {
				toastr.warning(result.message, "Warning");
			}
		},
		error: (error) => {
			toastr.error(error.message, "Error");
		}
	})
})

function loadItems() {
	$.ajax({
		url: `/Item/Items?page=${pagination?.page || 1}&limit=${pagination?.limit || 10}&itemName=${pagination.filterByItemName}&itemCategory=${pagination.filterByItemCategory}`,
		type: 'GET',
		success: (result) => {
			$("#item_list").html(result);
		},
		error: (error) => {

		}
	})

	$("#itemCategory").val(pagination.filterByItemCategory);
	$("#itemName").val(pagination.filterByItemName);
}

$("#filterForm").on("submit", (e) => {
	e.preventDefault();
	const itemCat = $("#itemCategory").val();
	const itemName = $("#itemName").val();

	if (pagination.filterByItemCategory === itemCat && pagination.filterByItemName === itemName) return;
	pagination.page = 1;

	pagination.filterByItemCategory = itemCat;
	pagination.filterByItemName = itemName;

	loadItems();
})

function paginate(page) {
	pagination.page = page;
	loadItems();
}