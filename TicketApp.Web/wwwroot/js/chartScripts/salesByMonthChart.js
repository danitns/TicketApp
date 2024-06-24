async function getDataAndCreateChartForMonthlySales() {
    var response = await fetch("/monthly-transactions", {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
    });

    if (response.ok) {
        const jsonResponse = await response.json();
        if (jsonResponse) {
            createChart(jsonResponse);
        }
    }

    function createChart(dataArray) {
        let labels = [];
        let data = [];
        for (let i in dataArray) {
            labels.push(dataArray[i].name);
            data.push(dataArray[i].noTransactions);
        }

        var parrentNode = document.getElementById('monthlyTransactionsContainer');
        

        new Chart(parrentNode, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: '# of sold tickets',
                    data: data,
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                },
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    title: {
                        display: true,
                        text: 'Monthly Sales'
                    }
                }
            }
        });
    }
}

getDataAndCreateChartForMonthlySales();