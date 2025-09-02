// Set new default font family and font color to mimic Bootstrap's default styling
(Chart.defaults.global.defaultFontFamily = "body-font"),
'body-font,sans-serif';
Chart.defaults.global.defaultFontColor = "#858796";

// Area Chart Example
var ctx = document.getElementById("myAreaChartUsers");


const labels = ['موبایل', 'تبلت', 'دسکتاپ', 'تلویزیون هوشمند', 'ناشناس'];
const data = {
    labels: labels,
    datasets: [
        {
            label: 'Dataset 1',
            data: [80, 75, 55, 35,15],
            backgroundColor: [
                "rgba(0, 97, 242, 0.5)",
                "rgba(0, 172, 105, 0.5)",
                "rgba(255,165,0,0.5)",
                "rgb(137,179,0,0.5)",
                "rgba(0, 97, 242, 0.5)",
            ]
        }
    ]
};


var myLineChart = new Chart(ctx, {
    type: 'polarArea',
    data: data,
    options: {
        responsive: true,
        scales: {
            r: {
                pointLabels: {
                    display: true,
                    centerPointLabels: true,
                    font: {
                        size: 45
                    }
                }
            }
        },
        plugins: {
            legend: {
                position: 'top',
            },
            title: {
                display: true,
                text: 'Chart.js Polar Area Chart With Centered Point Labels'
            }
        }
    },
});
