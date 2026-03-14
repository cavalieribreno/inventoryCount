// centralized fetch wrapper that adds JWT token to all requests
export async function apiFetch(url: string, options: RequestInit = {}) {
    const saved = localStorage.getItem("user");
    const token = saved ? JSON.parse(saved).token : null;

    const headers: HeadersInit = {
        "Content-Type": "application/json",
        ...options.headers,
    };
    // add Authorization header if token exists
    if (token) {
        (headers as Record<string, string>)["Authorization"] = `Bearer ${token}`;
    }

    const response = await fetch(url, { ...options, headers });

    // if token expired or invalid, logout and redirect to login
    if (response.status === 401) {
        localStorage.removeItem("user");
        window.location.href = "/";
    }

    return response;
}
