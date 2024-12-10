const list = document.querySelector('ul');
const form = document.getElementById('uploadForm');
const file = document.querySelector('input[type="file"]');
const deleteButton = document.getElementById("delete");
const updateButton = document.getElementById("update");
const searchField = document.getElementById("search");
const searchForm = document.getElementById("searchForm");
const searchClear = document.getElementById("clear");

let selected = null;

document.querySelector('ul').addEventListener('click', function(e) {
    if(e.target.tagName == 'LI') {
        selected = e.target;
        
        let otherSelected = document.querySelector('li.selected');                   
        if (otherSelected) otherSelected.classList.remove('selected');

        e.target.className = 'selected';
    }
});

form.onsubmit = (event) => {
    event.preventDefault();
    if(file.files.length === 0) return;
    
    const formData = new FormData(form);
    formData.append('file', file.files[0]);
    
    fetch('http://localhost:8081/documents', {
        method: 'POST',
        body: formData
    })
    .then(data => {
        fetchDocuments();
        form.reset();
    });
};

updateButton.onclick = (event) => {
    if(!selected) return;
    
    const newName = prompt("Enter the new name of the document:");
    updateDocument(selected.dataset.paperlessId, newName);
    selected = null;
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
};

const deleteDocument = (id) => {
    fetch('http://localhost:8081/documents?id=' + id, {
        method: 'DELETE',
    }).then(_ => fetchDocuments());
};

const fetchDocuments = () => {
    list.innerHTML = '';
    fetch('http://localhost:8081/documents')
    .then(response => response.json())
    .then(data => {
        data.forEach(doc => {
            const li = document.createElement('li');
            li.tabIndex = 1;
            li.textContent = doc.title;
            li.dataset.paperlessId = doc.id;
            list.appendChild(li);
        });
    });
}

const searchDocuments = (query) => {
    list.innerHTML = '';
    selected = null;
    fetch('http://localhost:8081/documents/search?q=' + query)
        .then(response => response.json())
        .then(data => {
            data.forEach(doc => {
                const li = document.createElement('li');
                li.tabIndex = 1;
                li.textContent = doc.title;
                li.dataset.paperlessId = doc.id;
                list.appendChild(li);
            });
        });
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

fetchDocuments();