const list = document.querySelector('ul');
const form = document.getElementById('uploadForm');
const file = document.querySelector('input[type="file"]');

const deleteButton = document.getElementById("delete");
const updateButton = document.getElementById("update");
const openButton = document.getElementById("open");
const downloadButton = document.getElementById("download");
const refreshButton = document.getElementById("refresh");
const viewButton = document.getElementById("view");

const searchField = document.getElementById("search");
const searchForm = document.getElementById("searchForm");
const searchClear = document.getElementById("clear");

const previewContainer = document.getElementById("previewContainer");
const preview = document.getElementById("preview");
const closePreviewButton = document.getElementById("closePreview");

let selected = null;
const pending = [];

class PaperlessWebSocket {
    constructor(url) {
        this.url = url;
        this.socket = null;
    }

    keepalive = () => {
        this.socket.send('ping');
        setTimeout(this.keepalive, 30000);
    }
    
    isopen = () => {
        return this.socket != null && this.socket.readyState === WebSocket.OPEN;
    }
    
    open() {
        if (this.socket && this.socket.readyState !== WebSocket.CLOSED) {
            console.warn("WebSocket is already open or connecting.");
            return;
        }

        this.socket = new WebSocket(this.url);

        this.socket.onmessage = (event) => {
            console.log(event.data);

            try {
                const data = JSON.parse(event.data);
                if(data.id === undefined) return;

                pending.splice(pending.indexOf(parseInt(data.id)), 1);
                if(pending.length === 0) this.close();
                fetchDocuments();
            } catch(_){}
        }

        this.socket.onopen = () => {
            console.log('Connected to server');
            this.socket.send("Hello, server!");
            this.keepalive();
        }
        
        this.socket.onclose = () => {
            console.log('Connection closed');
        }
        
        this.socket.onerror = (error) => {
            console.error('WebSocket Error: ' + error);
        }
    }

    close() {
        if (this.socket) {
            this.socket.close();
        }
    }
}

const ws = new PaperlessWebSocket('ws://localhost:8081/status');

window.onkeyup = (event) => {
    if(event.key === 'Delete' && selected) {
        deleteDocument(selected.dataset.paperlessId);
        selected = null;
        updateButtons();
    }
}

document.querySelector('ul').addEventListener('click', function(e) {
    if(e.target.tagName == 'LI') {
        if(selected === e.target) {
            e.target.classList.remove('selected');
            selected = null;
            updateButtons();
            return;
        }
        
        selected = e.target;
        updateButtons();
        
        let otherSelected = document.querySelector('li.selected');                   
        if (otherSelected) otherSelected.classList.remove('selected');

        e.target.className = 'selected';
    }
});

form.onsubmit = async (event) => {
    event.preventDefault();
    if(file.files.length === 0) return;
    
    for (const f of file.files) {
        const formData = new FormData(form);
        formData.append('file', f);

        const response = await fetch('http://localhost:8081/documents', {
            method: 'POST',
            body: formData
        });
        
        const id = await response.text();
        
        if(response.status !== 200) {
            const error = JSON.parse(id)[0];
            alert("Failed to upload document: " + error.errorMessage);
            form.reset();
            return;
        }
        
        pending.push(parseInt(id));
        ws.open();
    }
    
    fetchDocuments();
    form.reset();
};

const updateButtons = () => {
    if(selected) {
        deleteButton.disabled = false;
        updateButton.disabled = false;
        openButton.disabled = false;
        downloadButton.disabled = false;
        viewButton.disabled = false;
    } else {
        deleteButton.disabled = true;
        updateButton.disabled = true;
        openButton.disabled = true;
        downloadButton.disabled = true;
        viewButton.disabled = true;
    }
}

updateButton.onclick = (event) => {
    if(!selected) return;
    
    let newName = prompt("Enter the new name of the document:", selected.textContent);
    if(newName === null || newName === "") return;
    if(!newName.endsWith('.pdf')) newName += '.pdf';
    
    updateDocument(selected.dataset.paperlessId, newName);
    selected = null;
    updateButtons();
}

const updateDocument = (id, newName) => {
    const updateObj = {
        "Title": newName
    };

    fetch('http://localhost:8081/documents?id=' + id, {
        method: 'PUT',
        body: JSON.stringify(updateObj),
        headers: {'content-type': 'application/json'}
    }).then(_ => fetchDocuments());
}

deleteButton.onclick = (event) => {
      if(!selected) return;
      
      deleteDocument(selected.dataset.paperlessId);
      selected = null;
      updateButtons();
};

const deleteDocument = (id) => {
    fetch('http://localhost:8081/documents?id=' + id, {
        method: 'DELETE',
    }).then(_ => fetchDocuments());
};

const addDataToList = (data) => {
    data.sort((a, b) => a.uploadDate < b.uploadDate ? 1 : -1);
    data.forEach(doc => {
        const li = document.createElement('li');
        li.tabIndex = 1;
        li.textContent = doc.title;
        li.dataset.paperlessId = doc.id;
        if(pending.includes(doc.id) || !doc.content || doc.content === "") {
            li.classList.add('incomplete');
            li.textContent += " | OCR in progress";
            
            if(!ws.isopen()) ws.open();
        }
        list.appendChild(li);
    });
}

const fetchDocuments = () => {
    list.innerHTML = '';
    fetch('http://localhost:8081/documents')
    .then(response => response.json())
    .then(data => {
        addDataToList(data);
        
        if(list.children.length === 0) {
            let li = document.createElement('li');
            li.textContent = "No documents found";
            list.appendChild(li);
        }
    });
}

const searchDocuments = (query) => {
    if(query === "") return;
    
    list.innerHTML = '';
    selected = null;
    updateButtons();
    fetch('http://localhost:8081/documents/search?q=' + query)
        .then(response => response.json())
        .then(data => addDataToList(data));
}

searchForm.onsubmit = (event) => {
    event.preventDefault();
    searchDocuments(searchField.value);
}

searchField.oninput = (event) => {
    if(searchField.value === "") fetchDocuments();
}

clear.onclick = (event) => {
    if(searchField.value === "") return;

    searchField.value = "";
    fetchDocuments();
}

refreshButton.onclick = (event) => {
    fetchDocuments();
}

openButton.onclick = (event) => {
    if(!selected) return;
    
    fetch('http://localhost:8081/documents/download?id=' + selected.dataset.paperlessId)
        .then(response => response.blob())
        .then(blob => {
            let file = window.URL.createObjectURL(blob);
            window.open(file);
        });
};

// https://stackoverflow.com/questions/18650168/convert-blob-to-base64#answer-18650249
function blobToBase64(blob) {
    return new Promise((resolve, _) => {
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result);
        reader.readAsDataURL(blob);
    });
}

viewButton.onclick = (event) => {
    if(!selected) return;
    
    fetch('http://localhost:8081/documents/download?id=' + selected.dataset.paperlessId)
        .then(response => response.blob())
        .then(async blob => {
            blobToBase64(blob).then(data => {
                preview.src = data;
                previewContainer.style.display = "block";
            });
        });
};

closePreviewButton.onclick = (event) => {
    preview.src = "";
    previewContainer.style.display = "none";
};

downloadButton.onclick = (event) => {
    if(!selected) return;
    
    fetch('http://localhost:8081/documents/download?id=' + selected.dataset.paperlessId)
        .then(response => response.blob())
        .then(blob => {
            let url = window.URL.createObjectURL(blob);
            let a = document.createElement('a');
            a.href = url;
            a.download = selected.textContent;
            a.click();
        });
}

updateButtons();
fetchDocuments();

window.onbeforeunload = () => {
    console.log('Closing connection');
    ws.onclose = () => {
        console.log('Connection closed');
    };
    ws.close();
}