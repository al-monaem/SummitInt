var pagination = {
    filterByCategoryName: "",
    page: 1,
    limit: 10,
    totalPage: 0,
    totalItem: 0,
};

$(document).ready(function () {
    loadCategories();
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
        url: "/Category/BulkUpload",
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
                refreshNotification();
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

const categoryUrl = () => {
    return `/Category/GetCategories?page=${pagination.page}&limit=${pagination.limit}&categoryName=${pagination.filterByCategoryName}`
}

function loadCategories() {
    $.fn.dataTable.ext.errMode = 'none';
    var table = $('#cat_table').DataTable({
        paging: false,
        searching: false,
        info: false,
        ordering: false,
        language: {
            emptyTable: "<div style=\"height: 200px\" class=\"d-flex flex-column align-items-center justify-content-center\"><div><i class=\"bi bi-exclamation-triangle-fill h1\"></i></div><div class=\"text-body\">No data available</div></div>"
        },
        ajax: {
            dataSrc: ((data) => {
                pagination.page = data.data.pagination.page;
                pagination.limit = data.data.pagination.limit;
                pagination.totalItem = data.data.pagination.totalItem;
                pagination.totalPage = data.data.pagination.totalPage;
                generatePaginationHTML();

                return data.data.list;
            }),
            url: categoryUrl(),
            method: "GET",
        },
        columns: [
            { data: "id", width: "5%" },
            { data: "name" },
            {
                data: "createdBy",
                render: (data, type, row) => {
                    return row?.createdByUser?.userName
                }
            },
            {
                data: "createdAt",
                render: (data) => {
                    if (data)
                        return new Date(data).toLocaleString();
                }
            },
            {
                data: "updatedBy",
                render: (data, type, row) => {
                    return row?.updatedByUser?.userName
                }
            },
            {
                data: "updatedAt",
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
                },
                visible: userRole === userRoles.ADMIN,
            }
        ],
    });
}

$("#categoryCrudForm").on("submit", (e) => {
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
                console.log(result);
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
                if (error.status === 403) {
                    toastr.warning("You do not have permission to access this resource", 'Error!');
                }
                else {
                    toastr.error("Something Went Wrong! Please try again later!", 'Error!')
                }
            })
        });
    }
});

function generatePaginationHTML() {
    let html = '<div class="pagination d-flex gap-1">';
    const totalPages = pagination.totalPage;
    const currentPage = pagination.page;

    // Previous Button
    html += `<button class="btn btn-info ${currentPage === 1 || !pagination.totalPage ? "disabled" : ""}" onclick="paginate((${currentPage > 1 ? currentPage - 1 : 1}))">Previous</button>`;

    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, startPage + 4);

    for (let i = startPage; i <= endPage; i++) {
        html += '<button' + (i === currentPage || !pagination.totalPage ? ' class="disabled btn btn-primary"' : ' class="btn btn-info"') + ' onclick="paginate(' + i + ')">' + i + '</button>';
    }

    // Next Button
    html += `<button class="btn btn-info ${currentPage === totalPages || !pagination.totalPage ? "disabled" : ""}" onclick="paginate((${currentPage < totalPages ? currentPage + 1 : totalPages}))">Next</button>`;

    html += '</div>';

    $("#paginator").html(html);
}

function paginate(page) {
    console.log(page);
    pagination.page = page;
    $('#cat_table').DataTable().ajax.url(categoryUrl()).load();
}

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

$("#cat-search-form").on("submit", (e) => {
    e.preventDefault();

    const formData = new FormData(e.currentTarget);
    const formProps = Object.fromEntries(formData);

    const name = formProps.cat_name || "";
    
    if (pagination.filterByCategoryName !== name) {
        pagination.page = 1;
        pagination.filterByCategoryName = name;
        pagination.filterByCategoryName = name;
        $('#cat_table').DataTable().ajax.url(categoryUrl()).load();
    }
});