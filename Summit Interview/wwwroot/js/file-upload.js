var file = null;

window.addEventListener("dragover", function (e) {
	e.preventDefault();
}, false);
window.addEventListener("drop", function (e) {
	e.preventDefault();
}, false);

document.getElementById("custom-file-upload").addEventListener('drop', (e) => {
	e.preventDefault();
	e.stopPropagation();

	if (e.dataTransfer.files.length > 1) {
		toastr.error("Cannot upload multiple files", "Error");
	}

	file = e.dataTransfer.files[0];

	previewFile();
}, false);

$("#fileUploadBtn").on("click", (e) => {
	e.stopPropagation();
	$("#file_input").click();
})

$("#file_input").on("change", (e) => {
	if (e.currentTarget.files.length > 1) {
		toastr.error("Cannot upload multiple files", "Error");
	}
	file = e.currentTarget.files[0];
	previewFile();
})
function previewFile() {
	var html = `
		<div class="card rounded-1 mb-2">
			<div class="d-flex py-2 align-items-center">
				<div>
					<i class="bi bi-filetype-xlsx" style="margin-right: 4px;"></i>
				</div>
				<div>${file?.name}</div>
			</div>
		</div>
	`;
	$("#previewFile").html(html);
}

function clearFile() {
	file = null;
	document.getElementById("file_input").value = null;
}