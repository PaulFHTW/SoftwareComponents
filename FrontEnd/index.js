const list = document.querySelector('ul');
const form = document.querySelector('form');
const file = document.querySelector('input[type="file"]');

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

const fetchDocuments = () => {
    list.innerHTML = '';
    fetch('http://localhost:8081/documents')
    .then(response => response.json())
    .then(data => {
        data.forEach(doc => {
            const li = document.createElement('li');
            li.textContent = doc.title;
            list.appendChild(li);
        });
    });
}

fetchDocuments();