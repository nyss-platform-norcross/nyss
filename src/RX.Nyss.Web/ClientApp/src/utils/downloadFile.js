export const downloadFile = ({ url, fileName, data }) =>
  new Promise((resolve, reject) => {
    try {
      const xhr = new XMLHttpRequest();
      xhr.open("POST", url, true);
      xhr.withCredentials = true;
      xhr.responseType = "blob";
      xhr.timeout = 120_000;

      xhr.onload = () => {
        if (xhr.status !== 200) {
          reject(
            new Error(
              `Downloading file "${fileName}" failed. Status: ${xhr.statusText} (${xhr.status})`,
            ),
          );
          return;
        }
        try {
          const href = URL.createObjectURL(xhr.response);
          const link = document.createElement("a");
          link.href = href;
          link.download = fileName;
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
          setTimeout(() => URL.revokeObjectURL(href), 100);
          resolve();
        } catch (error) {
          reject(error);
        }
      };

      xhr.onerror = () =>
        reject(new Error(`Network error while downloading "${fileName}"`));

      xhr.ontimeout = () =>
        reject(new Error(`Timeout while downloading "${fileName}"`));

      xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");

      if (window.userLanguage) {
        xhr.setRequestHeader(
          "Accept-Language",
          `${window.userLanguage}, en;q=0.5`,
        );
      }

      xhr.send(data == null ? "{}" : JSON.stringify(data));
    } catch (e) {
      reject(e);
    }
  });