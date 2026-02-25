import axios from 'axios';

axios.defaults.baseURL = "http://localhost:5142";

// הוספת ה-Token לכל בקשה שיוצאת לשרת
axios.interceptors.request.use(config => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
// תפיסת שגיאות מהשרת (כמו טוקן שפג תוקפו)
axios.interceptors.response.use(
  response => response,
  error => {
    // ריענון רק אם זו שגיאת 401 (לא מורשה) והשגיאה היא לא מהלוגין עצמו
    if (error.response && error.response.status === 401 && !error.config.url.includes("/login")) {
      localStorage.removeItem("token");
      window.location.reload(); 
    }
    return Promise.reject(error);
  }
);

export default {
  // --- פונקציות משתמשים ---

  login: async (username, password) => {
    const res = await axios.post("/login", { Username: username, Password: password });
    localStorage.setItem("token", res.data.token);
    return res.data;
  },

  register: async (username, password) => {
    await axios.post("/register", { Username: username, Password: password });
  },

  // --- פונקציות משימות ---

  getTasks: async () => {
    const result = await axios.get(`/items`);
    return result.data;
  },

  addTask: async (name) => {
    const result = await axios.post(`/items`, { name: name, isComplete: false });
    return result.data;
  },

  setComplete: async (id, isComplete) => {
    await axios.put(`/items/${id}`, { isComplete: isComplete });
  },

  deleteTask: async (id) => {
    await axios.delete(`/items/${id}`);
  }
};