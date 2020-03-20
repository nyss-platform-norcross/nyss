export const downloadFile = ({ url, fileName, token , data}) =>
    new Promise((resolve, reject) => {
        try {
            const xhr = new XMLHttpRequest();
            xhr.open("POST", url, true);
            xhr.responseType = "blob";
            xhr.onreadystatechange = () => {
                if (xhr.readyState === 4) {
                    if (xhr.status !== 200) {
                        reject(new Error(`Downloading file "${fileName}" failed. Status: ${xhr.statusText} (${xhr.status})`));
                        return;
                    }
                    try {
                        const href = URL.createObjectURL(xhr.response);
                        const link = document.createElement("a");
                        link.href = href;
                        link.download = fileName;
                        link.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true, view: window }));
                        resolve();
                    } catch (error) {
                        reject(error);
                    }
                }
            };
            xhr.setRequestHeader('Content-type', 'application/json; charset=utf-8');
            xhr.send(JSON.stringify(data));
        } catch (e) {
            reject(e);
        }
    });
