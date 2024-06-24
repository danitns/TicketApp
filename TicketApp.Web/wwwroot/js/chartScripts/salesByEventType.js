async function getDataAndCreateChartForEventTypeSales() {
    var response = await fetch("/transactions-by-type", {
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

        var parrentNode = document.getElementById('salesByEventTypeContainer');

        new Chart(parrentNode, {
            type: 'pie',
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
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    title: {
                        display: true,
                        text: 'Sales by event type'
                    }
                }
            }
        });
    }

}




getDataAndCreateChartForEventTypeSales();