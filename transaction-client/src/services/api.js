import axios from 'axios';

const BASE_API_URL = process.env.REACT_APP_BASE_API_URL;

export async function getBooks() {
    try {
        const response = await axios.get(`${process.env.REACT_APP_BOOKS_API_URL}`);
        return response.data;
    } catch (error) {
        console.error('Error fetching books:', error);
        throw error;
    }
}

export async function getClients() {
    try {
        const response = await axios.get(`${process.env.REACT_APP_USERS_API_URL}`);
        return response.data;
    } catch (error) {
        console.error('Error fetching clients:', error);
        throw error;
    }
}

export async function submitPurchaseRequest(purchaseRequest) {
    try {
        const response = await axios.post(`${process.env.REACT_APP_PURCHASE_BOOK_API_URL}`, purchaseRequest);
        return response.data;
    } catch (error) {
        console.error('Error submitting purchase request:', error);
        throw error.response ? error.response.data : "Error connecting to API";
    }
}
