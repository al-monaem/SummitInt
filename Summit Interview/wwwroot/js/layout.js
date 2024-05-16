$(document).ready(() => {
    refreshNotification();
    setInterval(() => {
        refreshNotification();
    }, 5000)
});

function refreshNotification() {
    $.ajax({
        url: "/Notification/GetFileUploadsNotification",
        type: "GET",
        success: (result) => {
            $("#notification-count").html(result.data.length);
            var html = `<h6 class="p-2" style="margin: 0px;">Notifications</h6><hr class="m-0 mb-1"/>`;
            if (result.data.length === 0) html += `<div style="padding: 8px !important;">No new notifications</div>`;
            else {
                for (const n of result.data) {
                    console.log(n);
                    html += `
                        <div class="mx-2 p-2 d-flex gap-2">
                            <div style="max-width: 20px;"><i class="bi bi-file-earmark-fill"></i></div>
                            <div class="d-flex flex-column gap-2">
                                <div style="font-size: 12px;">${n.filename}</div>
                                <div class="d-flex" style="color: white; font-size: 10px;">${n.uploadStatus === 0 ?
                                    `<span style="background-color: var(--bs-info); border-radius: 4px;" class="p-1 text-center d-flex gap-1">
                                        Uploading <div class="spinner-border spinner-border-sm text-white" style="font-size: 10px;" role="status">
                                                    <span class="visually-hidden text-center">L</span>
                                                </div>
                                    </span>`
                                    : n.uploadStatus === 1 ?
                                    `<span style="background-color: var(--bs-primary); border-radius: 4px;" class="p-1 text-center d-flex gap-1">
                                        Completed <i class="bi bi-check2-circle"></i>
                                    </span>`
                                    : `<span style="background-color: var(--bs-danger); border-radius: 4px;" class="p-1 text-center d-flex gap-1">
                                            Failed <i class="bi bi-x-octagon-fill"></i>
                                        </span>`}
                                </div>
                            </div>
                        </div>
                    `;
                }
            }
            $("#notifications").html(html);
        },
        error: (error) => {
            console.log(erorr)
            $("#notification-count").html(0);
        }
    });
}