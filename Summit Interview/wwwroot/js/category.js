$(document).ready(function () {
    loadCategories({
        page: 1,
        limit: 10,
    });
})

function loadCategories(pagination) {
    $.fn.dataTable.ext.errMode = 'none';
    $('#cat_table').DataTable({
        paging: false,
        ordering: false,
        searching: false,
        info: false,
        language: {
            emptyTable: "<div style=\"height: 200px\" class=\"d-flex flex-column align-items-center justify-content-center\"><div><i class=\"bi bi-exclamation-triangle-fill h1\"></i></div><div class=\"text-body\">No data available</div></div>"
        },
        ajax: {
            url: `/Category/GetCategories?page=${pagination.page}&limit=${pagination.limit}`,
            method: "GET",
        },
        columns: [
            { data: "id", width: "5%" },
            { data: "name" },
            { data: "createdBy" },
            {
                data: "createdAt",
                render: (data) => {
                    if (data)
                        return new Date(data).toLocaleString();
                }
            },
            {
                data: "id",
                render: (data, type, row) => {
                    return `<div class="d-flex">
                        <button onclick="editCategory(${row.id}, '${row.name}')" data-bs-toggle="modal" data-bs-target="#categoryCrudModal" data-categoryName="${row.name}" data-categoryId="${row.id}" style="margin-right:8px;" class="btn btn-primary">Edit</button>
                        <button onclick="deleteCategory(${data})" class="btn btn-danger">Delete</button>
                    </div>`
                }
            }
        ],
    });
}

$(document).on("submit", "#categoryCrudForm", (e) => {
    {
        e.preventDefault();
        $("#name_error").html("");
        $("#category_name").removeClass("is-invalid")

        const name = $("#category_name").val();
        const id = $("#category_id").val() || 0;

        $.ajax({
            url: id ? "/Category/UpdateCategory" : "/Category/CreateCategory",
            method: id ? "PUT" : "POST",
            data: {
                id,
                name,
            },
            success: ((result) => {
                if (result.status === 201) {
                    toastr.success(result.message, 'Success!')
                    $("#categoryCrudModal").modal("toggle");
                    $('#cat_table').DataTable().ajax.reload();
                } else {
                    if (result.status === 410) {
                        if (Object.keys(result.message).includes("Name") && result.message?.Name?.errors?.length) {
                            $("#category_name").addClass("is-invalid");
                            $("#name_error").html(`*${result.message?.Name?.errors?.[0]?.errorMessage}`);
                        }
                    } else if (result.status >= 500) {
                        toastr.error(result.message, 'Error!')
                    } else {
                        toastr.warning(result.message, 'Error!')
                    }
                }
            }),
            error: ((error) => {
                toastr.error("Something Went Wrong! Please try again later!", 'Error!')
            })
        });
    }
})

function deleteCategory(id) {
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
                url: `/Category/DeleteCategory/${id}`,
                type: "DELETE",
                success: (result) => {
                    if (result.status === 201) {
                        toastr.success(result.message, "Deleted");
                        $('#cat_table').DataTable().ajax.reload();
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

function editCategory(id, name) {
    $("#category_id").val(id);
    $("#category_name").val(name);
    $("#categoryCrudModalLabel").html("Update Category");
}

function clearInput() {
    $("#category_name").val('');
    $("#name_error").html("");
    $("#category_name").removeClass("is-invalid")
    $("#category_id").val('');
    $("#categoryCrudModalLabel").html("Create Category");
}

$("#categoryCrudModal").on("hidden.bs.modal", () => {
    clearInput();
})