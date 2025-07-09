$(document).ready(function () {
    loadCustomerBookingPieChart();
});
function loadCustomerBookingPieChart() {
    $(".chart-spinner").show();
    $.ajax({
        url: "/Dashboard/GetBookingsPieChartData",
        type: 'GET',
        dataType: 'json',
        success: function (data) {
           
            loadPieChart("customerBookingsPieChart", data);

            $(".chart-spinner").hide();
        }
    });
}
function loadPieChart(id, data) {
    var chartColors = getChartColorsArray(id);
    options = {
        colors: chartColors,
        series: data.series,
        labels: data.labels,
        chart: {
            width: 380,
            type: 'pie'
        },
        legend: {
            position: 'bottom',
            horizontalAign: 'center',
            labels: {
                colors: "#fff",
                useSeriesColors: true
            },
        },
    };
    var chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}