let { createApp, ref } = Vue;
let textDecoder = new TextDecoder();
function convertReaderValue(value) {
    let dataStr = textDecoder.decode(value);
    if (dataStr.startsWith(',') || dataStr.startsWith('['))
        dataStr = dataStr.slice(1).trim();
    if (dataStr.endsWith(']'))
        dataStr = dataStr.slice(0, -1).trim();
    dataStr = `[${dataStr}]`;
    try {
        return JSON.parse(dataStr);
    }
    catch (err) {
        console.error('Error process data:', dataStr);
        return [];
    }
}

createApp({
    setup() {
        let phoneNumber = ref('');
        let results = ref([]);
        let loading = ref(false);
        let error = ref(null);
        let checkedClientNames = ref([]);
        let clientOptions = ref([]);

        fetch('/Home/GetClientNames')
            .then(response => response.json())
            .then(jsonData => {
                clientOptions.value = jsonData;
                checkedClientNames.value = jsonData.map(x => x.value);
            });

        let queryPhone = async () => {
            if (!phoneNumber.value) {
                error.value = '請輸入號碼'
                return;
            }
            if (!checkedClientNames.value.length) {
                error.value = '請選取查詢來源'
                return;
            }

            loading.value = true;
            error.value = null;
            results.value = [];

            try {
                let condition = {
                    Phone: phoneNumber.value,
                    ClientNames: checkedClientNames.value
                };
                await fetch('/Home/QueryPhone', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(condition)
                }).then((response) => {
                    let reader = response.body.getReader();
                    function processReaderResult({ done, value }) {
                        if (done) {
                            loading.value = false;
                            return
                        }
                        let jsonData = convertReaderValue(value);
                        results.value.push(...jsonData);
                        reader.read().then(processReaderResult);
                    }
                    reader.read().then(processReaderResult);
                });
            }
            catch (err) {
                error.value = '查詢失敗，請稍後再試';
                console.error('Error:', err);
            }
        }

        return {
            phoneNumber, results,
            loading, error,
            clientOptions, checkedClientNames,
            queryPhone
        }
    }
}).mount('#app');
