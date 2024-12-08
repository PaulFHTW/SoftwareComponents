const list = document.querySelector('ul');
const form = document.querySelector('form');
const file = document.querySelector('input[type="file"]');
const deleteButton = document.getElementById("delete");

var selected = null;

document.querySelector('ul').addEventListener('click', function(e) {
    if(e.target.tagName == 'LI') {
        selected = e.target;
        
        var otherSelected = document.querySelector('li.selected');                   
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

    updateDocument(selected.dataset.paperlessId);
    selected = null;
}

const updateDocument = (id) => {
    fetch('http://localhost:8081/documents?id=' + id, {
        method: 'PUT',
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

fetchDocuments();