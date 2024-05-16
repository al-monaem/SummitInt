var make_select;
var existingImages = [];
var selectedFiles = [];
var slider;

window.addEventListener("dragover", function (e) {
	e.preventDefault();
}, false);
window.addEventListener("drop", function (e) {
	e.preventDefault();
}, false);

$(document).ready(() => {
    $('#lightSlider').lightSlider({
        gallery: true,
        item: 1,
        loop: true,
        slideMargin: 0,
        thumbItem: 6
	});

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

function slide(dir) {
	if (dir === "r") {
		slider.goToNextSlide();
	} else {
		slider.goToPrevSlide();
	}
}

$("#customFileUploadBtn").on("click", (e) => {
	e.stopPropagation();
	$("#item_images").click();
})

$("#item_images").on("change", (e) => {
	selectedFiles.push(...e.currentTarget.files);
	document.getElementById("item_images").value = null;
	previewImage();
})

function handleEditModal(categoryName, categoryId, itemImages) {
	make_select.addOption({
		value: categoryId,
		text: categoryName,
	});
	make_select.setValue(categoryId);

	existingImages = itemImages.map(itemImage => itemImage.ImageUrl)
	previewImage();
}

function previewImage() {
	let htmlString = "";

	if (existingImages?.length) {
		for (let i = 0; i < existingImages.length; i++) {
			htmlString += `
			<div class="position-relative">
				<img class="img-thumbnail"
					style="height:100px;width:100px;padding:0px;margin-right:5px;margin-bottom:10px;"
					src="/${existingImages[i]}" />
				<div onclick="removeImage('old', ${i})" class="close-btn">
					<i class="bi bi-x-circle-fill"></i>
				</div>
			</div>
		`
		}
	}

	if (selectedFiles?.length) {
		for (let i = 0; i < selectedFiles.length; i++) {
			htmlString += `
			<div class="position-relative">
				<img class="img-thumbnail"
					style="height:100px;width:100px;padding:0px;margin-right:5px;margin-bottom:10px;"
					src="${URL.createObjectURL(selectedFiles[i])}" />
				<div onclick="removeImage('new', ${i})" class="close-btn">
					<i class="bi bi-x-circle-fill"></i>
				</div>
			</div>
		`
		}
	}

	$("#previewRow").html(htmlString);
}

function removeImage(type, index) {
	if (type === "new") {
		selectedFiles.splice(index, 1);
		previewImage();
	} else {
		existingImages.splice(index, 1);
		previewImage();
	}
}

$("#itemEditForm").on("submit", (e) => {
	e.preventDefault();

	const formData = new FormData(e.currentTarget);
	const formProps = Object.fromEntries(formData);

	console.log(formProps);

	const payload = new FormData();
	payload.append("categoryId", formProps.Category || "");
	payload.append("id", formProps.Id);
	payload.append("itemName", formProps.ItemName || "");
	payload.append("itemUnit", formProps.ItemUnit || "");
	payload.append("itemQuantity", formProps.ItemQuantity || "");
	payload.append("itemDescription", tinymce.activeEditor.getContent() || "");

	if (existingImages.length === 0) {
		payload.append("existingImages", []);
	}

	for (var i = 0; i != existingImages.length; i++) {
		payload.append("existingImages", existingImages[i]);
	}

	for (var i = 0; i != selectedFiles.length; i++) {
		payload.append("files", selectedFiles[i]);
	}

	$.ajax({
		url: "/Item/Update",
		type: "PUT",
		data: payload,
		processData: false,
		contentType: false,
		success: (result) => {
			if (result.status === 201) {
				toastr.success(result.message, "Success");
				$("#itemEditModal").modal("toggle");
				window.location.reload();
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

function handleDeleteItem(id) {
	Swal.fire({
		title: "Are you sure?",
		text: "You won't be able to revert this!",
		icon: "warning",
		showCancelButton: true,
		confirmButtonText: "Yes, delete it!",
		cancelButtonText: "No, cancel!",
		reverseButtons: true,
		heightAuto: false,
	}).then((result) => {
		if (result.isConfirmed) {
			$.ajax({
				url: `/Item/Delete/${id}`,
				type: "DELETE",
				success: (result) => {
					if (result.status === 201) {
						toastr.success(result.message, "Deleted");
						window.location.href = "/";
					} else if (result.status >= 500) {
						toastr.error(result.message, "Error");
					} else {
						toastr.warning(result.message, "Error");
					}
				},
				error: (error) => {
					toastr.error(error.message, "Error");
				}
			});
		}
	});
}