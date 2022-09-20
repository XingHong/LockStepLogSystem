using System;
public class CircularQueue<T> 
{
    private int front;
    private int rear;
    private int capacity;
    private T[] elements;
    public CircularQueue(int capacity)
    {
        this.capacity = capacity + 1;
        elements = new T[this.capacity];
        front = 0;
        rear = 0;
    }

    public int Count 
    { 
        get
        {
            return (rear - front + capacity) % capacity;
        }
    }

    public T Dequeue()
    {
        if (IsEmpty())
            throw new ArgumentOutOfRangeException("circular queue is empty."); 
        T item = elements[front];
        front = (front + 1) % capacity;
        return item;
    }

    public void Enqueue(T item)
    {
        if (IsFull())
            throw new ArgumentOutOfRangeException("circular queue is full.");
        elements[rear] = item;
        rear = (rear + 1) % capacity;
    }

    public T GetNextItem()
    {
        if (IsFull())
            throw new ArgumentOutOfRangeException("circular queue is full.");  
        return elements[rear];
    }

    public void Clear()
    {
        front = 0;
        rear = 0;
    }

    public T FrontItem()
    {
        if (IsEmpty())
            throw new ArgumentOutOfRangeException("circular queue is empty.");  
        return elements[front];
    }

    public T RearItem()
    {
        if (IsEmpty())
            throw new ArgumentOutOfRangeException("circular queue is empty.");  
        return elements[(rear - 1 + capacity) % capacity];
    }

    public bool IsEmpty()
    {
        return front == rear;
    }

    public bool IsFull()
    {
        return front == ((rear + 1) % capacity);
    }
    
}